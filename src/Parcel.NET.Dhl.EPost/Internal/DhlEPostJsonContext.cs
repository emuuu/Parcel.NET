using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.EPost.Internal;

[JsonSerializable(typeof(DhlEPostLoginRequest))]
[JsonSerializable(typeof(DhlEPostLoginResponse))]
[JsonSerializable(typeof(DhlEPostLetter[]))]
[JsonSerializable(typeof(DhlEPostLetterIdent[]))]
[JsonSerializable(typeof(DhlEPostLetterStatus))]
[JsonSerializable(typeof(DhlEPostLetterStatus[]))]
[JsonSerializable(typeof(long[]))]
[JsonSerializable(typeof(DhlEPostError))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DhlEPostJsonContext : JsonSerializerContext;
