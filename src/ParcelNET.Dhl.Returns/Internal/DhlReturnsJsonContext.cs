using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.Returns.Internal;

[JsonSerializable(typeof(DhlReturnOrderRequest))]
[JsonSerializable(typeof(DhlReturnOrderResponse))]
[JsonSerializable(typeof(DhlReturnLocationResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DhlReturnsJsonContext : JsonSerializerContext;
