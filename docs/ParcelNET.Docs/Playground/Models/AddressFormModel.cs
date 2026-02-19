using ParcelNET.Abstractions.Models;

namespace ParcelNET.Docs.Playground.Models;

public class AddressFormModel
{
    public string Name { get; set; } = "";
    public string Street { get; set; } = "";
    public string? HouseNumber { get; set; }
    public string PostalCode { get; set; } = "";
    public string City { get; set; } = "";
    public string CountryCode { get; set; } = "DEU";

    public Address ToAddress() => new()
    {
        Name = Name,
        Street = Street,
        HouseNumber = HouseNumber,
        PostalCode = PostalCode,
        City = City,
        CountryCode = CountryCode
    };

    public static AddressFormModel DefaultShipper() => new()
    {
        Name = "Sender GmbH",
        Street = "Senderstrasse",
        HouseNumber = "1",
        PostalCode = "10115",
        City = "Berlin",
        CountryCode = "DEU"
    };

    public static AddressFormModel DefaultConsignee() => new()
    {
        Name = "Max Mustermann",
        Street = "Empfaengerstrasse",
        HouseNumber = "42",
        PostalCode = "80331",
        City = "Munich",
        CountryCode = "DEU"
    };
}
