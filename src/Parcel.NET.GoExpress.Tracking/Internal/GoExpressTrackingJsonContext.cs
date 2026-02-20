using System.Text.Json.Serialization;

namespace Parcel.NET.GoExpress.Tracking.Internal;

[JsonSerializable(typeof(GoExpressTrackingResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class GoExpressTrackingJsonContext : JsonSerializerContext;
