using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Internal;

[JsonSerializable(typeof(DhlTokenService.TokenResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class DhlAuthJsonContext : JsonSerializerContext;
