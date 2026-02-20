using Parcel.NET.Dhl.Pickup.Models;

namespace Parcel.NET.Docs.Playground.Models;

public class DhlCreatePickupFormModel
{
    public string BillingNumber { get; set; } = "33333333330101";
    public DateOnly PickupDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    public bool UseLocationId { get; set; }
    public string LocationId { get; set; } = "";

    // Address
    public string Name { get; set; } = "Sender GmbH";
    public string Street { get; set; } = "Senderstrasse";
    public string HouseNumber { get; set; } = "1";
    public string PostalCode { get; set; } = "10115";
    public string City { get; set; } = "Berlin";
    public string Country { get; set; } = "DE";

    // Contact
    public string ContactName { get; set; } = "Max Mustermann";
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    // Shipment
    public string TransportationType { get; set; } = "PAKET";
    public double? TotalWeightInKg { get; set; }
    public string? Comment { get; set; }

    public PickupOrderRequest ToRequest()
    {
        var location = new PickupLocation();
        if (UseLocationId && !string.IsNullOrWhiteSpace(LocationId))
        {
            location.LocationId = LocationId;
        }
        else
        {
            location.Address = new PickupAddress
            {
                Name1 = Name,
                Street = Street,
                HouseNumber = HouseNumber,
                PostalCode = PostalCode,
                City = City,
                Country = Country
            };
        }

        return new PickupOrderRequest
        {
            BillingNumber = BillingNumber,
            PickupDate = PickupDate,
            Location = location,
            TotalWeightInKg = TotalWeightInKg,
            Comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment,
            ContactPersons = [new PickupContactPerson
            {
                Name = ContactName,
                Phone = string.IsNullOrWhiteSpace(ContactPhone) ? null : ContactPhone,
                Email = string.IsNullOrWhiteSpace(ContactEmail) ? null : ContactEmail
            }],
            Shipments = [new PickupShipment { TransportationType = TransportationType }]
        };
    }
}
