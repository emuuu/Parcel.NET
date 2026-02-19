using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.Pickup.Internal;

[JsonSerializable(typeof(DhlPickupOrderRequest))]
[JsonSerializable(typeof(DhlPickupOrderResponse))]
[JsonSerializable(typeof(DhlPickupOrderDetailsResponse))]
[JsonSerializable(typeof(DhlPickupCancellationResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DhlPickupJsonContext : JsonSerializerContext;
