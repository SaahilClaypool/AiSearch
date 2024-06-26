using OpenAI_API;
using OpenAI_API.Chat;

namespace AiSearch;

public interface ILLM
{
    Task<T?> Complete<T>(string prompt, string? system = null);
    IAsyncEnumerable<string> Stream(string prompt, string? system = null);
}

public class Groq(string key)
    : OpenAI(key, "https://api.groq.com/openai/{0}/{1}", llama38b)
{
    const string llama370b = "llama3-70b-8192";
    const string llama38b = "llama3-8b-8192";
}

public class OpenAI(string key, string url, string model) : ILLM
{
    const string System = """
        Exclude all introductions. Answer concisely and use well-formatted markdown.
        """;
    OpenAIAPI oai = new(key) { ApiUrlFormat = url };

    public async Task<T?> Complete<T>(string prompt, string? system)
    {
        Console.WriteLine($"Complete: {typeof(T).Name}\n{prompt}");
        var request = Request();
        request.ResponseFormat = ChatRequest.ResponseFormats.JsonObject;
        request.Messages ??= [];
        request.Messages.Add(new ChatMessage(ChatMessageRole.User, prompt));
        var response = await oai.Chat.CreateChatCompletionAsync(request);
        var json = response.Choices[0].Message.TextContent;
        return json.FromJson<T>();
    }

    ChatRequest Request() =>
        new ChatRequest()
        {
            Model = model,
            ResponseFormat = ChatRequest.ResponseFormats.Text,
            StopSequence = "TERMINATE",
            Temperature = 0.5,
            TopP = 1,
        };

    public async IAsyncEnumerable<string> Stream(string prompt, string? system)
    {
        system ??= System;
        Console.WriteLine($"Key is : {key}");
        Console.WriteLine($"Stream: {prompt} {system}");
        var request = Request();
        request.Model = model;
        request.Messages ??= [];
        var systemMessage = new ChatMessage
        {
            Role = ChatMessageRole.User,
            TextContent = system
        };
        request.Messages.Add(systemMessage);
        request.Messages.Add(new ChatMessage(ChatMessageRole.User, prompt));
        var response = oai.Chat.StreamChatEnumerableAsync(request);
        await foreach (var token in response)
        {
            var content = token.Choices[0].Delta.TextContent;
            yield return content;
        }
    }
}

public class MockLLM() : ILLM
{
    public async Task<T?> Complete<T>(string prompt, string? system)
    {
        await Task.Delay(TimeSpan.FromSeconds(0.1));
        return default;
    }

    public async IAsyncEnumerable<string> Stream(
        string prompt,
        string? system = null
    )
    {
        foreach (
            var token in "the quick brown fox jumped over the lazy dogs".Split()
        )
        {
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            yield return token + " ";
        }
    }
}
