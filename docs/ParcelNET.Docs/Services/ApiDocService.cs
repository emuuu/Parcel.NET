using System.Net.Http.Json;
using System.Text.Json;
using ParcelNET.Docs.Models;

namespace ParcelNET.Docs.Services;

public sealed class ApiDocService
{
    private readonly HttpClient _httpClient;
    private Dictionary<string, ApiTypeDoc>? _apiDocs;
    private bool _initialized;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiDocService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        var docs = await _httpClient.GetFromJsonAsync<List<ApiTypeDoc>>("api-docs.json", JsonOptions)
            ?? [];

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
