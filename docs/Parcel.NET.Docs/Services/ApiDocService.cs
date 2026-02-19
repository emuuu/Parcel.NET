using System.Text.Json;
using Parcel.NET.Docs.Models;

namespace Parcel.NET.Docs.Services;

public sealed class ApiDocService
{
    private readonly IWebHostEnvironment _env;
    private Dictionary<string, ApiTypeDoc>? _apiDocs;
    private bool _initialized;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiDocService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        var path = Path.Combine(_env.WebRootPath, "api-docs.json");
        var json = await File.ReadAllTextAsync(path);
        var docs = JsonSerializer.Deserialize<List<ApiTypeDoc>>(json, JsonOptions) ?? [];

        _apiDocs = docs.ToDictionary(d => d.TypeName, StringComparer.OrdinalIgnoreCase);
        _initialized = true;
    }

    public ApiTypeDoc? GetApiDoc(string typeName)
    {
        return _apiDocs?.GetValueOrDefault(typeName);
    }

    public List<ApiTypeDoc> GetAllTypes()
    {
        return _apiDocs?.Values.OrderBy(t => t.TypeName).ToList() ?? [];
    }
}
