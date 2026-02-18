namespace ParcelNET.Docs.Models;

public sealed class ContentIndexEntry
{
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public required string Category { get; set; }
    public int Order { get; set; }
    public string? Description { get; set; }
    public string? ApiRef { get; set; }
    public List<string> Headings { get; set; } = [];
    public string SearchText { get; set; } = "";
}
