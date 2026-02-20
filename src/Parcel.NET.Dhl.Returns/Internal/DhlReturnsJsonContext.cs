using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Returns.Internal;

[JsonSerializable(typeof(DhlReturnOrderRequest))]
[JsonSerializable(typeof(DhlReturnOrderResponse))]
[JsonSerializable(typeof(List<DhlReturnLocationItem>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DhlReturnsJsonContext : JsonSerializerContext;
