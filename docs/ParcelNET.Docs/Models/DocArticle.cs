namespace ParcelNET.Docs.Models;

public sealed class DocArticle
{
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string HtmlContent { get; set; }
    public List<HeadingInfo> Headings { get; set; } = [];
    public string? ApiRefType { get; set; }
}
