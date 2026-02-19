using ParcelNET.GoExpress.Shipping.Models;

namespace ParcelNET.Docs.Playground.Models;

public class GoExpressCreateShipmentFormModel
{
    public GoExpressService Service { get; set; } = GoExpressService.ON;
    public GoExpressLabelFormat LabelFormat { get; set; } = GoExpressLabelFormat.PdfA4;
    public AddressFormModel Shipper { get; set; } = AddressFormModel.DefaultShipper();
    public AddressFormModel Consignee { get; set; } = AddressFormModel.DefaultConsignee();
    public PackageFormModel Package { get; set; } = new();
    public string? Reference { get; set; }
    public string? Content { get; set; } = "Test shipment";
    public DateOnly PickupDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    public TimeOnly? PickupTimeFrom { get; set; } = new(8, 0);
    public TimeOnly? PickupTimeTill { get; set; } = new(17, 0);
    public bool SelfPickup { get; set; }
    public bool FreightCollect { get; set; }
    public bool IdentCheck { get; set; }
    public bool ReceiptNotice { get; set; }

    public GoExpressShipmentRequest ToRequest() => new()
    {
        Service = Service,
        LabelFormat = LabelFormat,
        Shipper = Shipper.ToAddress(),
        Consignee = Consignee.ToAddress(),
        Packages = [Package.ToPackage()],
        Reference = Reference,
        Content = Content,
        Pickup = new TimeWindow
        {
            Date = PickupDate,
            TimeFrom = PickupTimeFrom,
            TimeTill = PickupTimeTill
        },
        SelfPickup = SelfPickup,
        FreightCollect = FreightCollect,
        IdentCheck = IdentCheck,
        ReceiptNotice = ReceiptNotice
    };
}
