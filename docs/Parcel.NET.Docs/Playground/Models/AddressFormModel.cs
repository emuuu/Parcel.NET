using Parcel.NET.Abstractions.Models;

namespace Parcel.NET.Docs.Playground.Models;

public class AddressFormModel
{
    public string Name { get; set; } = "";
    public string? Name2 { get; set; }
    public string? Name3 { get; set; }
    public string? Street { get; set; } = "";
    public string? HouseNumber { get; set; }
    public string PostalCode { get; set; } = "";
    public string City { get; set; } = "";
    public string CountryCode { get; set; } = "DEU";
    public string? State { get; set; }

    public Address ToAddress() => new()
    {
        Name = Name,
        Name2 = string.IsNullOrWhiteSpace(Name2) ? null : Name2,
        Name3 = string.IsNullOrWhiteSpace(Name3) ? null : Name3,
        Street = string.IsNullOrWhiteSpace(Street) ? null : Street,
        HouseNumber = HouseNumber,
        PostalCode = PostalCode,
        City = City,
        CountryCode = CountryCode,
        State = string.IsNullOrWhiteSpace(State) ? null : State
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
