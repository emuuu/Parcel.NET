using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Pickup.Internal;

[JsonSerializable(typeof(DhlPickupOrderRequest))]
[JsonSerializable(typeof(DhlPickupOrderResponse))]
[JsonSerializable(typeof(DhlPickupCancellationResponse))]
[JsonSerializable(typeof(DhlPickupOrderStatus[]))]
[JsonSerializable(typeof(DhlPickupLocationInfo[]))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DhlPickupJsonContext : JsonSerializerContext;
