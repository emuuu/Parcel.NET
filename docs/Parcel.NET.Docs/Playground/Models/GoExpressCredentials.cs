namespace Parcel.NET.Docs.Playground.Models;

public class GoExpressCredentials
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public string ResponsibleStation { get; set; } = "";

    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(CustomerId);
}
