using System.Text.Json.Serialization;

namespace Parcel.NET.GoExpress.Shipping.Internal;

[JsonSerializable(typeof(GoExpressOrderRequest))]
[JsonSerializable(typeof(GoExpressOrderResponse))]
[JsonSerializable(typeof(GoExpressUpdateStatusRequest))]
[JsonSerializable(typeof(GoExpressLabelRequest))]
[JsonSerializable(typeof(GoExpressLabelResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class GoExpressShippingJsonContext : JsonSerializerContext;
