namespace AiSearch;

public record ConversationState
{
    public DateTime StartTime = DateTime.Now;
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<MessageState> Messages { get; set; } = [];
}

public record MessageState
{
    public required string UserInput { get; set; }
    public string? Query { get; set; }
    public List<SearchLink>? SearchResults { get; set; }
    public string? LLMOutput { get; set; }
    public bool Complete { get; set; }
}
