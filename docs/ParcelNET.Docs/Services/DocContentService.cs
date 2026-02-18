using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ParcelNET.Docs.Models;

namespace ParcelNET.Docs.Services;

public sealed partial class DocContentService
{
    private readonly HttpClient _httpClient;
    private readonly MarkdownPipeline _pipeline;
    private readonly IDeserializer _yamlDeserializer;
    private List<ContentIndexEntry>? _index;
    private bool _initialized;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DocContentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseAutoLinks()
            .UseTaskLists()
            .Build();
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _index = await _httpClient.GetFromJsonAsync<List<ContentIndexEntry>>("content-index.json", JsonOptions)
            ?? [];
        _initialized = true;
    }

    public List<NavSection> GetNavSections()
    {
        if (_index is null) return [];

        return _index
            .GroupBy(e => e.Category)
            .Select(g => new NavSection
            {
                Category = g.Key,
                Items = g.OrderBy(e => e.Order).ToList()
            })
            .OrderBy(s => GetCategoryOrder(s.Category))
            .ToList();
    }

    public async Task<DocArticle?> GetArticleAsync(string slug)
    {
        await InitializeAsync();

        var entry = _index?.FirstOrDefault(e =>
            e.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

        try
        {
            var markdown = await _httpClient.GetStringAsync($"content/{slug}.md");
            var (frontMatter, body) = ParseFrontMatter(markdown);
            var html = Markdown.ToHtml(body, _pipeline);
            var headings = ExtractHeadings(html);

            return new DocArticle
            {
                Slug = slug,
                Title = frontMatter?.Title ?? entry?.Title ?? slug,
                Description = frontMatter?.Description ?? entry?.Description,
                HtmlContent = html,
                Headings = headings,
                ApiRefType = frontMatter?.ApiRef ?? entry?.ApiRef
            };
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public List<SearchResult> Search(string query)
    {
        if (_index is null || string.IsNullOrWhiteSpace(query))
            return [];

        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var results = new List<SearchResult>();

        foreach (var entry in _index)
        {
            var score = 0;
            foreach (var term in terms)
            {
                if (entry.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
                    score += 10;
                if (entry.Headings.Any(h => h.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    score += 5;
                if (entry.SearchText.Contains(term, StringComparison.OrdinalIgnoreCase))
                    score += 1;
                if (entry.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)
                    score += 3;
            }

            if (score > 0)
            {
                results.Add(new SearchResult
                {
                    Slug = entry.Slug,
                    Title = entry.Title,
                    Description = entry.Description,
                    Headings = entry.Headings,
                    Score = score
                });
            }
        }

        return results.OrderByDescending(r => r.Score).Take(20).ToList();
    }

    private (FrontMatter?, string) ParseFrontMatter(string markdown)
    {
        if (!markdown.StartsWith("---"))
            return (null, markdown);

        var endIndex = markdown.IndexOf("---", 3);
        if (endIndex < 0) return (null, markdown);

        var yaml = markdown[3..endIndex].Trim();
        var body = markdown[(endIndex + 3)..].TrimStart();

        try
        {
            var fm = _yamlDeserializer.Deserialize<FrontMatter>(yaml);
            return (fm, body);
        }
        catch
        {
            return (null, markdown);
        }
    }

    private static List<HeadingInfo> ExtractHeadings(string html)
    {
        var headings = new List<HeadingInfo>();
        var regex = HeadingRegex();

        foreach (Match match in regex.Matches(html))
        {
            var level = int.Parse(match.Groups[1].Value);
            var id = match.Groups[2].Value;
            var text = Regex.Replace(match.Groups[3].Value, "<[^>]+>", "").Trim();

            if (string.IsNullOrEmpty(id))
                id = Regex.Replace(text.ToLowerInvariant(), @"[^\w]+", "-").Trim('-');

            if (!string.IsNullOrEmpty(text))
            {
                headings.Add(new HeadingInfo
                {
                    Id = id,
                    Text = text,
                    Level = level
                });
            }
        }

        return headings;
    }

    private static int GetCategoryOrder(string category) => category switch
    {
        "Getting Started" => 0,
        "Carriers" => 1,
        "Guides" => 2,
        "API Reference" => 3,
        _ => 99
    };

    [GeneratedRegex(@"<h([23])\s*(?:id=""([^""]*)"")?\s*[^>]*>(.*?)</h[23]>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex HeadingRegex();
}

public sealed class FrontMatter
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public int? Order { get; set; }
    public string? Description { get; set; }
    public string? ApiRef { get; set; }
}

public sealed class NavSection
{
    public required string Category { get; set; }
    public List<ContentIndexEntry> Items { get; set; } = [];
}

public sealed class SearchResult
{
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public List<string> Headings { get; set; } = [];
    public int Score { get; set; }
}
