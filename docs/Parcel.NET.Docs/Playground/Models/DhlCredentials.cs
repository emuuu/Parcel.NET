namespace Parcel.NET.Docs.Playground.Models;

public class DhlCredentials
{
    // Shared
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";

    // Shipping / Pickup / Returns (ROPC)
    public string Username { get; set; } = "user-valid";
    public string Password { get; set; } = "SandboxPasswort2023!";

    // Tracking XML (Parcel DE Tracking v0)
    public string TrackingUsername { get; set; } = "zt12345";
    public string TrackingPassword { get; set; } = "geheim";

    // Internetmarke (may differ from shipping credentials)
    public string InternetmarkeUsername { get; set; } = "";
    public string InternetmarkePassword { get; set; } = "";

    public bool IsShippingComplete =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(ApiSecret) &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);

    public bool IsTrackingComplete =>
        !string.IsNullOrWhiteSpace(ApiKey);

    public bool IsPickupComplete => IsShippingComplete;

    public bool IsReturnsComplete => IsShippingComplete;

    public bool IsInternetmarkeComplete =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(ApiSecret) &&
        !string.IsNullOrWhiteSpace(InternetmarkeUsername) &&
        !string.IsNullOrWhiteSpace(InternetmarkePassword);

    public bool IsLocationFinderComplete =>
        !string.IsNullOrWhiteSpace(ApiKey);

    public bool IsTrackingXmlComplete =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(TrackingUsername) &&
        !string.IsNullOrWhiteSpace(TrackingPassword);
}
