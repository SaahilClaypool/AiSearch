using Newtonsoft.Json.Linq;
using RestSharp;

namespace AiSearch;

public class Searcher(string key)
{
    public async Task<List<SearchLink>> Search(string query)
    {
        var client = new RestClient("https://google.serper.dev");
        var request = new RestRequest("search", Method.Post);
        request.AddHeader("X-API-KEY", key);
        request.AddHeader("Content-Type", "application/json");
        var body = new { q = query };
        request.AddParameter(
            "application/json",
            body,
            ParameterType.RequestBody
        );
        var response = await client.ExecuteAsync(request);
        Console.WriteLine($"Searched for {query} - received:");
        Console.WriteLine(response.Content);
        var objects = response.Content!.FromJson<SerperTypes.RootObject>()!;
        return objects
            .Organic!.Select(o => new SearchLink(
                o.Title!,
                o.Link!,
                o.Description ?? o.Snippet!
            ))
            .ToList();
    }
}

public record SearchLink(string Title, string Url, string Description);

internal static class SerperTypes
{
    public class SearchParameters
    {
        public string? Q { get; set; }
        public string? Type { get; set; }
        public string? Engine { get; set; }
    }

    public class SearchInformation
    {
        public string? DidYouMean { get; set; }
    }

    public class AnswerBox
    {
        public string? Snippet { get; set; }
        public List<string>? SnippetHighlighted { get; set; }
        public string? Title { get; set; }
        public string? Link { get; set; }
    }

    public class OrganicResult
    {
        public string? Title { get; set; }
        public string? Link { get; set; }
        public string? Description { get; set; }
        public string? Snippet { get; set; }
        public DateTime? Date { get; set; }
        public List<Sitelink>? Sitelinks { get; set; }
        public int? Position { get; set; }
    }

    public class Sitelink
    {
        public string? Title { get; set; }
        public string? Link { get; set; }
    }

    public class Attributes
    {
        public string? Duration { get; set; }
        public string? Posted { get; set; }
    }

    public class RootObject
    {
        public SearchParameters? SearchParameters { get; set; }
        public SearchInformation? SearchInformation { get; set; }
        public AnswerBox? AnswerBox { get; set; }
        public List<OrganicResult>? Organic { get; set; }
        public List<PeopleAlsoAsk>? PeopleAlsoAsk { get; set; }
        public List<RelatedSearches>? RelatedSearches { get; set; }
    }

    public class PeopleAlsoAsk
    {
        public string? Question { get; set; }
        public string? Snippet { get; set; }
        public string? Title { get; set; }
        public string? Link { get; set; }
    }

    public class RelatedSearches
    {
        public string? Query { get; set; }
    }
}
