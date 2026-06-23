using System.Text.Json;
using System.Text.Json.Serialization;

namespace Parcel.NET.LetterXpress.Letters.Internal;

// Requests
[JsonSerializable(typeof(LxAuthRequest))]
[JsonSerializable(typeof(LxLetterRequest))]
[JsonSerializable(typeof(LxPriceRequest))]
[JsonSerializable(typeof(LxUpdateRequest))]
[JsonSerializable(typeof(LxEmailUpdateRequest))]
// Responses
[JsonSerializable(typeof(LxResponse<LxBalanceData>))]
[JsonSerializable(typeof(LxResponse<LxPriceData>))]
[JsonSerializable(typeof(LxResponse<LxPrintJobWire>))]
[JsonSerializable(typeof(LxResponse<LxPrintJobsData>))]
[JsonSerializable(typeof(LxResponse<LxEmailJobWire>))]
[JsonSerializable(typeof(LxResponse<LxEmailCreateData>))]
[JsonSerializable(typeof(LxResponse<LxTransactionsData>))]
[JsonSerializable(typeof(LxResponse<LxInvoicesData>))]
[JsonSerializable(typeof(LxResponse<LxInvoiceWire>))]
[JsonSerializable(typeof(LxResponse<JsonElement>))]
[JsonSerializable(typeof(JsonElement))]
// Standalone types for JsonElement.Deserialize on polymorphic e-mail responses.
[JsonSerializable(typeof(LxEmailJobWire))]
[JsonSerializable(typeof(LxPrintJobWire))]
[JsonSerializable(typeof(LxEmailCreateData))]
[JsonSerializable(typeof(LxPaginationWire))]
// WICHTIG: snake_case-Namenskonvention (base64_file, email_receiver, dispatch_date …).
// WhenWritingNull, damit optionale Felder weggelassen werden.
// AllowReadingFromString, falls die API numerische Werte als Strings liefert.
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    NumberHandling = JsonNumberHandling.AllowReadingFromString)]
internal partial class LetterXpressJsonContext : JsonSerializerContext;
