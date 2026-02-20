using Parcel.NET.Abstractions.Models;
using Parcel.NET.Dhl.Shipping.Models;

namespace Parcel.NET.Docs.Playground.Models;

public class DhlCreateShipmentFormModel
{
    public string BillingNumber { get; set; } = "33333333330101";
    public DhlProduct Product { get; set; } = DhlProduct.V01PAK;
    public AddressFormModel Shipper { get; set; } = AddressFormModel.DefaultShipper();
    public AddressFormModel Consignee { get; set; } = AddressFormModel.DefaultConsignee();
    public PackageFormModel Package { get; set; } = new();
    public string? Reference { get; set; }

    // Consignee type
    public DhlConsigneeType ConsigneeType { get; set; } = DhlConsigneeType.ContactAddress;
    public int? LockerId { get; set; }
    public string? PostNumber { get; set; }
    public int? RetailId { get; set; }
    public int? PoBoxId { get; set; }

    // Label options
    public LabelFormat LabelFormat { get; set; } = LabelFormat.Pdf;
    public DhlPrintFormat PrintFormat { get; set; } = DhlPrintFormat.A4;

    // Value-added services
    public string? PreferredDay { get; set; }
    public string? PreferredLocation { get; set; }
    public string? PreferredNeighbour { get; set; }
    public decimal? InsuredValue { get; set; }
    public bool BulkyGoods { get; set; }
    public bool NamedPersonOnly { get; set; }
    public bool NoNeighbourDelivery { get; set; }
    public bool Premium { get; set; }

    // Customs
    public bool HasCustoms { get; set; }
    public string ExportType { get; set; } = "COMMERCIAL_GOODS";
    public string? InvoiceNo { get; set; }
    public string CustomsItemDescription { get; set; } = "";
    public string CustomsItemOrigin { get; set; } = "DEU";
    public int CustomsItemQuantity { get; set; } = 1;
    public double CustomsItemWeight { get; set; }
    public decimal CustomsItemValue { get; set; }

    public DhlShipmentRequest ToRequest()
    {
        var dhlConsignee = ConsigneeType != DhlConsigneeType.ContactAddress
            ? new DhlConsignee
            {
                Type = ConsigneeType,
                LockerId = LockerId,
                PostNumber = PostNumber,
                RetailId = RetailId,
                PoBoxId = PoBoxId
            }
            : null;

        var vas = HasVas()
            ? new DhlValueAddedServices
            {
                PreferredDay = string.IsNullOrWhiteSpace(PreferredDay) ? null : PreferredDay,
                PreferredLocation = string.IsNullOrWhiteSpace(PreferredLocation) ? null : PreferredLocation,
                PreferredNeighbour = string.IsNullOrWhiteSpace(PreferredNeighbour) ? null : PreferredNeighbour,
                InsuredValue = InsuredValue,
                BulkyGoods = BulkyGoods,
                NamedPersonOnly = NamedPersonOnly,
                NoNeighbourDelivery = NoNeighbourDelivery,
                Premium = Premium
            }
            : null;

        var labelOptions = new DhlLabelOptions
        {
            Format = LabelFormat,
            PrintFormat = PrintFormat
        };

        var customs = HasCustoms && !string.IsNullOrWhiteSpace(CustomsItemDescription)
            ? new DhlCustomsDetails
            {
                ExportType = ExportType,
                InvoiceNo = string.IsNullOrWhiteSpace(InvoiceNo) ? null : InvoiceNo,
                Items =
                [
                    new DhlCustomsItem
                    {
                        Description = CustomsItemDescription,
                        CountryOfOrigin = CustomsItemOrigin,
                        Quantity = CustomsItemQuantity,
                        Weight = CustomsItemWeight,
                        Value = CustomsItemValue
                    }
                ]
            }
            : null;

        return new DhlShipmentRequest
        {
            Profile = "STANDARD_GRUPPENPROFIL",
            BillingNumber = BillingNumber,
            Product = Product,
            Shipper = Shipper.ToAddress(),
            Consignee = Consignee.ToAddress(),
            DhlConsignee = dhlConsignee,
            Packages = [Package.ToPackage()],
            Reference = Reference,
            LabelOptions = labelOptions,
            ValueAddedServices = vas,
            CustomsDetails = customs
        };
    }

    private bool HasVas() =>
        !string.IsNullOrWhiteSpace(PreferredDay) ||
        !string.IsNullOrWhiteSpace(PreferredLocation) ||
        !string.IsNullOrWhiteSpace(PreferredNeighbour) ||
        InsuredValue.HasValue ||
        BulkyGoods || NamedPersonOnly || NoNeighbourDelivery || Premium;
}
