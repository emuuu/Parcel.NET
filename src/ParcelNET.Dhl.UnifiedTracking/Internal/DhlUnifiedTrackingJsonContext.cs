using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.UnifiedTracking.Internal;

[JsonSerializable(typeof(DhlUnifiedTrackingResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class DhlUnifiedTrackingJsonContext : JsonSerializerContext;
