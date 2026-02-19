using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.LocationFinder.Internal;

[JsonSerializable(typeof(DhlLocationFinderResponse))]
[JsonSerializable(typeof(DhlLocationFinderSingleResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class DhlLocationFinderJsonContext : JsonSerializerContext;
