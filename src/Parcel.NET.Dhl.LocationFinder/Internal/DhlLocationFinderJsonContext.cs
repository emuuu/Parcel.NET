using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.LocationFinder.Internal;

[JsonSerializable(typeof(DhlLocationFinderResponse))]
[JsonSerializable(typeof(DhlLocationFinderSingleResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class DhlLocationFinderJsonContext : JsonSerializerContext;
