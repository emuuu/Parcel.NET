using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.UnifiedTracking.Internal;

[JsonSerializable(typeof(DhlUnifiedTrackingResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class DhlUnifiedTrackingJsonContext : JsonSerializerContext;
