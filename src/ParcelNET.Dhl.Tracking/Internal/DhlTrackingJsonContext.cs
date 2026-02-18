using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.Tracking.Internal;

[JsonSerializable(typeof(DhlTrackingResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class DhlTrackingJsonContext : JsonSerializerContext;
