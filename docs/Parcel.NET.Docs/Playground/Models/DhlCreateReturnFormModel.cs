using Parcel.NET.Dhl.Returns.Models;

namespace Parcel.NET.Docs.Playground.Models;

public class DhlCreateReturnFormModel
{
    public string ReceiverId { get; set; } = "deu";
    public string? CustomerReference { get; set; }
    public ReturnLabelType LabelType { get; set; } = ReturnLabelType.Both;

    // Shipper (person returning)
    public string Name1 { get; set; } = "Max Mustermann";
    public string? Name2 { get; set; }
    public string? Name3 { get; set; }
    public string AddressStreet { get; set; } = "Retourenstrasse";
    public string? AddressHouse { get; set; } = "1";
    public string PostalCode { get; set; } = "10115";
    public string City { get; set; } = "Berlin";
    public string Country { get; set; } = "deu";
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public int? WeightInGrams { get; set; }

    public ReturnOrderRequest ToRequest() => new()
    {
        ReceiverId = ReceiverId,
        CustomerReference = string.IsNullOrWhiteSpace(CustomerReference) ? null : CustomerReference,
        LabelType = LabelType,
        WeightInGrams = WeightInGrams,
        Shipper = new ReturnShipper
        {
            Name1 = Name1,
            Name2 = string.IsNullOrWhiteSpace(Name2) ? null : Name2,
            Name3 = string.IsNullOrWhiteSpace(Name3) ? null : Name3,
            AddressStreet = AddressStreet,
            AddressHouse = string.IsNullOrWhiteSpace(AddressHouse) ? null : AddressHouse,
            PostalCode = PostalCode,
            City = City,
            Country = Country,
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email,
            Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone
        }
    };
}
