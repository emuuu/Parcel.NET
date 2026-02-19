using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Internetmarke.Internal;

[JsonSerializable(typeof(DhlImTokenResponse))]
[JsonSerializable(typeof(DhlImUserProfileResponse))]
[JsonSerializable(typeof(DhlImCatalogResponse))]
[JsonSerializable(typeof(DhlImShoppingCartResponse))]
[JsonSerializable(typeof(DhlImCheckoutRequest))]
[JsonSerializable(typeof(DhlImCheckoutResponse))]
[JsonSerializable(typeof(DhlImWalletChargeRequest))]
[JsonSerializable(typeof(DhlImRetoureRequest))]
[JsonSerializable(typeof(DhlImRetoureResponse))]
[JsonSerializable(typeof(DhlImRetoureStateResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DhlInternetmarkeJsonContext : JsonSerializerContext;
