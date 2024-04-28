using System.Text;

namespace AiSearch;

public class Converser(
    ILLM llm,
    Searcher searcher,
    Dictionary<Guid, ConversationState> convos,
    IBackgroundTaskQueue backgroundTaskQueue
)
{
    public ConversationState Converasation(Guid id) => convos[id];

    public Guid StartConversation(string query)
    {
        var convo = new ConversationState();
        convos[convo.Id] = convo;
        convo.Messages.Add(new() { UserInput = query });
        ReplyInBackground(convo).Wait();
        return convo.Id;
    }

    public void Reply(Guid id, string message)
    {
        var convo = convos[id];
        convo.Messages.Add(new() { UserInput = message });
        ReplyInBackground(convo).Wait();
    }

    record SuggestQuery
    {
        public required string Query { get; set; }
    }

    public async Task<string> GetQuery(string input, string? previousMessage)
    {
        var prompt = $$"""
            Here is a conversation the user is having.
            The user wants us to answer or do the following: {{input}}

            Respond with the best google search query to provide information on results to help with that task.

            Examples:
            ---
            Input: Write a function in c# to invert a linked list
            Output { "Query": "stackoverflow.com c# invert linked list" }

            Input: Where was george washington born?
            Output { "Query": "wikipedia.org george washington place of birth" }

            Input: Where can I adopt dogs in Florida?
            Output { "Query": "Adopt dogs florida" }
            ---

            And here is some previous conversation context: {{previousMessage}}

            Now, here is users request - what search would you perform to answer the question or request: {{input}}
            Output your response in JSON with a single field for the "Query"
            """;
        var query = await llm.Complete<SuggestQuery>(prompt);
        return query?.Query ?? input;
    }

    async Task ReplyInBackground(ConversationState conversation)
    {
        var message = conversation.Messages[^1];
        await backgroundTaskQueue.QueueBackgroundWorkItemAsync(async ct =>
        {
            var previousContext = (conversation.Messages.Count > 1).Then(
                $"""
                ---
                {conversation.Messages[..^1].Select(m => $"""
                    User: {m.UserInput}

                    {LLMSearchResult(m.SearchResults ?? [], m.Query ?? m.UserInput)}

                    Assistant: {m.LLMOutput}
                    """).Join("\n")}
                ---
                """
            );
            message.Query = await GetQuery(message.UserInput, previousContext);
            message.SearchResults = await searcher.Search(message.UserInput);
            var sb = new StringBuilder();
            var prompt = $"""
            User Query: {message.UserInput}

            Context:

            {previousContext}

            {LLMSearchResult(message.SearchResults, message.Query)}

            Now, answer the user query:
            """;
            var llmResponse = llm.Stream(prompt);
            await foreach (var token in llmResponse)
            {
                sb.Append(token);
                // TODO: event?
                message.LLMOutput = sb.ToString();
            }
            message.Complete = true;
        });
    }

    string LLMSearchResult(List<SearchLink> links, string query) =>
        $"""
        Search Results for {query}:
        ---
        {links.Select(l => $"""
            - [{l.Title}]({l.Url})

                {l.Description}

            """).Join("\n")}
        ---
        """;
}
