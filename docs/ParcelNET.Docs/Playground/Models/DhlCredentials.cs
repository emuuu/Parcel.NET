namespace ParcelNET.Docs.Playground.Models;

public class DhlCredentials
{
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";
    public string Username { get; set; } = "user-valid";
    public string Password { get; set; } = "SandboxPasswort2023!";

    public bool IsShippingComplete =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(ApiSecret) &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);

    public bool IsTrackingComplete =>
        !string.IsNullOrWhiteSpace(ApiKey);
}
