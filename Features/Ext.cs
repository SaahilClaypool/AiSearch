namespace AiSearch;

public static partial class Ext
{
    public static string Join(this IEnumerable<string> items, string joiner) =>
        string.Join(joiner, items);

    public static string Then(
        this bool condition,
        string left,
        string? right = null
    ) => condition ? left : right ?? string.Empty;

    public static string ToJson(this object o, bool indented = false)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(
            o,
            indented
                ? Newtonsoft.Json.Formatting.Indented
                : Newtonsoft.Json.Formatting.None
        );
    }

    public static T? FromJson<T>(this string s)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s);
    }
}
