using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Shipping.Internal;

[JsonSerializable(typeof(DhlOrderRequest))]
[JsonSerializable(typeof(DhlOrderResponse))]
[JsonSerializable(typeof(DhlManifestRequest))]
[JsonSerializable(typeof(DhlApiContactAddress))]
[JsonSerializable(typeof(DhlApiLocker))]
[JsonSerializable(typeof(DhlApiPostOffice))]
[JsonSerializable(typeof(DhlApiPOBox))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DhlShippingJsonContext : JsonSerializerContext;
