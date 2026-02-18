using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var wwwrootPath = args.Length > 0 ? args[0] : throw new ArgumentException("wwwroot path required");
var baseUrl = "https://emuuu.github.io/ParcelNET";

for (var i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--base-url")
        baseUrl = args[i + 1];
}

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

Console.WriteLine($"Generating docs data in: {wwwrootPath}");

var contentIndex = await GenerateContentIndex(wwwrootPath);
await GenerateApiDocs(wwwrootPath);
GenerateSitemap(wwwrootPath, contentIndex, baseUrl);

Console.WriteLine("Done.");

async Task<List<ContentIndexEntry>> GenerateContentIndex(string wwwrootPath)
{
    var contentDir = Path.Combine(wwwrootPath, "content");
    var outputPath = Path.Combine(wwwrootPath, "content-index.json");

    if (!Directory.Exists(contentDir))
    {
        Console.WriteLine("No content directory found, skipping content index.");
        await File.WriteAllTextAsync(outputPath, "[]");
        return [];
    }

    var pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseAutoLinks()
        .UseTaskLists()
        .Build();

    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    var mdFiles = Directory.GetFiles(contentDir, "*.md", SearchOption.AllDirectories);
    var entries = new List<ContentIndexEntry>();

    foreach (var filePath in mdFiles)
    {
        var markdown = await File.ReadAllTextAsync(filePath);
        var (frontMatter, body) = ParseFrontMatter(markdown, deserializer);

        var relativePath = Path.GetRelativePath(contentDir, filePath)
            .Replace('\\', '/')
            .Replace(".md", "");

        var html = Markdown.ToHtml(body, pipeline);
        var headings = ExtractHeadings(html);
        var plainText = StripHtml(html);
        var searchText = plainText.Length > 500 ? plainText[..500] : plainText;

        entries.Add(new ContentIndexEntry
        {
            Slug = relativePath,
            Title = frontMatter?.Title ?? Path.GetFileNameWithoutExtension(filePath),
            Category = frontMatter?.Category ?? InferCategory(relativePath),
            Order = frontMatter?.Order ?? 99,
            Description = frontMatter?.Description,
            ApiRef = frontMatter?.ApiRef,
            Headings = headings,
            SearchText = searchText
        });
    }

    entries = entries.OrderBy(e => e.Category).ThenBy(e => e.Order).ToList();

    var json = JsonSerializer.Serialize(entries, jsonOptions);
    await File.WriteAllTextAsync(outputPath, json);
    Console.WriteLine($"Generated content-index.json with {entries.Count} entries.");
    return entries;
}

async Task GenerateApiDocs(string wwwrootPath)
{
    var outputPath = Path.Combine(wwwrootPath, "api-docs.json");
    var apiDocs = new List<ApiTypeDocEntry>();

    var assemblies = new[]
    {
        typeof(ParcelNET.Abstractions.IShipmentService).Assembly,
        typeof(ParcelNET.Dhl.DhlOptions).Assembly,
        typeof(ParcelNET.Dhl.Shipping.IDhlShippingClient).Assembly,
        typeof(ParcelNET.Dhl.Tracking.DhlTrackingClient).Assembly
    };

    var xmlDocs = new Dictionary<string, XDocument>();
    foreach (var asm in assemblies)
    {
        var xmlPath = Path.ChangeExtension(asm.Location, ".xml");
        if (File.Exists(xmlPath))
        {
            xmlDocs[asm.GetName().Name!] = XDocument.Load(xmlPath);
            Console.WriteLine($"Loaded XML docs: {xmlPath}");
        }
    }

    foreach (var asm in assemblies)
    {
        var types = asm.GetExportedTypes()
            .Where(t => !t.Name.Contains("Internal") && !t.Name.EndsWith("JsonContext"))
            .OrderBy(t => t.Name);

        foreach (var type in types)
        {
            var members = new List<ApiMemberEntry>();
            var asmName = asm.GetName().Name!;
            xmlDocs.TryGetValue(asmName, out var xmlDoc);

            // Public properties
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                members.Add(new ApiMemberEntry
                {
                    Name = prop.Name,
                    Kind = "Property",
                    Type = FormatType(prop.PropertyType),
                    Description = GetXmlSummary(xmlDoc, $"P:{type.FullName}.{prop.Name}")
                });
            }

            // Public methods (exclude getters/setters/object methods)
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && m.DeclaringType != typeof(object)))
            {
                var paramTypes = string.Join(", ", method.GetParameters().Select(p => FormatType(p.ParameterType)));
                members.Add(new ApiMemberEntry
                {
                    Name = $"{method.Name}({paramTypes})",
                    Kind = "Method",
                    Type = FormatType(method.ReturnType),
                    Description = GetXmlSummary(xmlDoc, $"M:{type.FullName}.{method.Name}")
                });
            }

            // Events
            foreach (var evt in type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                members.Add(new ApiMemberEntry
                {
                    Name = evt.Name,
                    Kind = "Event",
                    Type = FormatType(evt.EventHandlerType!),
                    Description = GetXmlSummary(xmlDoc, $"E:{type.FullName}.{evt.Name}")
                });
            }

            // Static extension methods
            if (type.IsAbstract && type.IsSealed) // static class
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => !m.IsSpecialName))
                {
                    var paramTypes = string.Join(", ", method.GetParameters().Select(p => FormatType(p.ParameterType)));
                    members.Add(new ApiMemberEntry
                    {
                        Name = $"{method.Name}({paramTypes})",
                        Kind = "Method",
                        Type = FormatType(method.ReturnType),
                        Description = GetXmlSummary(xmlDoc, $"M:{type.FullName}.{method.Name}")
                    });
                }
            }

            if (members.Count > 0)
            {
                apiDocs.Add(new ApiTypeDocEntry
                {
                    TypeName = type.Name,
                    FullName = type.FullName!,
                    Members = members
                });
            }
        }
    }

    var json = JsonSerializer.Serialize(apiDocs, jsonOptions);
    await File.WriteAllTextAsync(outputPath, json);
    Console.WriteLine($"Generated api-docs.json with {apiDocs.Count} types.");
}

static void GenerateSitemap(string wwwrootPath, List<ContentIndexEntry> entries, string baseUrl)
{
    var outputPath = Path.Combine(wwwrootPath, "sitemap.xml");
    var urls = new List<string> { baseUrl + "/" };
    urls.AddRange(entries.Select(e => $"{baseUrl}/docs/{e.Slug}"));

    var xml = $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
        {string.Join("\n", urls.Select(u => $"  <url><loc>{u}</loc></url>"))}
        </urlset>
        """;

    File.WriteAllText(outputPath, xml);
    Console.WriteLine($"Generated sitemap.xml with {urls.Count} URLs.");
}

static (FrontMatter?, string) ParseFrontMatter(string markdown, IDeserializer deserializer)
{
    if (!markdown.StartsWith("---"))
        return (null, markdown);

    var endIndex = markdown.IndexOf("---", 3);
    if (endIndex < 0)
        return (null, markdown);

    var yaml = markdown[3..endIndex].Trim();
    var body = markdown[(endIndex + 3)..].TrimStart();

    try
    {
        var fm = deserializer.Deserialize<FrontMatter>(yaml);
        return (fm, body);
    }
    catch
    {
        return (null, markdown);
    }
}

static List<string> ExtractHeadings(string html)
{
    var headings = new List<string>();
    var regex = new Regex(@"<h[23][^>]*>(.*?)</h[23]>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    foreach (Match match in regex.Matches(html))
    {
        var text = Regex.Replace(match.Groups[1].Value, "<[^>]+>", "").Trim();
        if (!string.IsNullOrEmpty(text))
            headings.Add(text);
    }
    return headings;
}

static string StripHtml(string html)
{
    var text = Regex.Replace(html, "<[^>]+>", " ");
    text = Regex.Replace(text, @"\s+", " ");
    return text.Trim();
}

static string InferCategory(string slug)
{
    var parts = slug.Split('/');
    if (parts.Length < 2) return "General";
    return parts[0].Replace("-", " ");
}

static string FormatType(Type type)
{
    if (type == typeof(void)) return "void";
    if (type == typeof(string)) return "string";
    if (type == typeof(int)) return "int";
    if (type == typeof(bool)) return "bool";
    if (type == typeof(double)) return "double";
    if (type == typeof(byte[])) return "byte[]";

    if (type.IsGenericType)
    {
        var name = type.Name.Split('`')[0];
        var args = string.Join(", ", type.GetGenericArguments().Select(FormatType));

        if (name == "Nullable")
            return $"{args}?";
        if (name == "Task" && args.Length > 0)
            return $"Task<{args}>";

        return $"{name}<{args}>";
    }

    var typeName = type.Name;
    if (Nullable.GetUnderlyingType(type) is { } underlying)
        return $"{FormatType(underlying)}?";

    return typeName;
}

static string? GetXmlSummary(XDocument? xmlDoc, string memberName)
{
    if (xmlDoc is null) return null;
    var member = xmlDoc.Descendants("member")
        .FirstOrDefault(m => m.Attribute("name")?.Value == memberName);
    var summary = member?.Element("summary")?.Value.Trim();
    return string.IsNullOrWhiteSpace(summary) ? null : Regex.Replace(summary, @"\s+", " ").Trim();
}

class FrontMatter
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public int? Order { get; set; }
    public string? Description { get; set; }
    public string? ApiRef { get; set; }
}

class ContentIndexEntry
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

class ApiTypeDocEntry
{
    public required string TypeName { get; set; }
    public required string FullName { get; set; }
    public List<ApiMemberEntry> Members { get; set; } = [];
}

class ApiMemberEntry
{
    public required string Name { get; set; }
    public required string Kind { get; set; }
    public required string Type { get; set; }
    public string? Description { get; set; }
}
