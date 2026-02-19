using ParcelNET.Dhl.Shipping.Models;

namespace ParcelNET.Docs.Playground.Models;

public class DhlCreateShipmentFormModel
{
    public string BillingNumber { get; set; } = "33333333330101";
    public DhlProduct Product { get; set; } = DhlProduct.V01PAK;
    public AddressFormModel Shipper { get; set; } = AddressFormModel.DefaultShipper();
    public AddressFormModel Consignee { get; set; } = AddressFormModel.DefaultConsignee();
    public PackageFormModel Package { get; set; } = new();
    public string? Reference { get; set; }

    public DhlShipmentRequest ToRequest() => new()
    {
        BillingNumber = BillingNumber,
        Product = Product,
        Shipper = Shipper.ToAddress(),
        Consignee = Consignee.ToAddress(),
        Packages = [Package.ToPackage()],
        Reference = Reference
    };
}
