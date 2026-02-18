using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.Shipping.Internal;

[JsonSerializable(typeof(DhlOrderRequest))]
[JsonSerializable(typeof(DhlOrderResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DhlShippingJsonContext : JsonSerializerContext;
