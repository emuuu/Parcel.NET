using System.Text.Json.Serialization;

namespace Parcel.NET.GoExpress.Shipping.Internal;

[JsonSerializable(typeof(GoExpressOrderRequest))]
[JsonSerializable(typeof(GoExpressOrderResponse))]
[JsonSerializable(typeof(GoExpressUpdateStatusRequest))]
[JsonSerializable(typeof(GoExpressLabelRequest))]
// WICHTIG: WhenWritingNull ist gesetzt, weil die GO! Express API leere Strings ""
// erwartet statt fehlender Felder. Alle Mapping-Methoden MÜSSEN null → "" konvertieren.
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class GoExpressShippingJsonContext : JsonSerializerContext;
