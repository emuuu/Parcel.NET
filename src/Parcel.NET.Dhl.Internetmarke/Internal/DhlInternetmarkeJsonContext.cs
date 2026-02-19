using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Internetmarke.Internal;

[JsonSerializable(typeof(DhlImUserInfoResponse))]
[JsonSerializable(typeof(DhlImCatalogResponse))]
[JsonSerializable(typeof(DhlImCartRequest))]
[JsonSerializable(typeof(DhlImCartResponse))]
[JsonSerializable(typeof(DhlImCheckoutResponse))]
[JsonSerializable(typeof(DhlImWalletResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DhlInternetmarkeJsonContext : JsonSerializerContext;
