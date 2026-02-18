using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.Internal;

[JsonSerializable(typeof(DhlTokenService.TokenResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class DhlAuthJsonContext : JsonSerializerContext;
