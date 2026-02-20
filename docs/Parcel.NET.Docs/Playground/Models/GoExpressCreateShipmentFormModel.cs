using Parcel.NET.GoExpress.Shipping.Models;

namespace Parcel.NET.Docs.Playground.Models;

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
    public TimeOnly? PickupAvisFrom { get; set; }
    public TimeOnly? PickupAvisTill { get; set; }
    public WeekendOrHolidayIndicator PickupWeekendOrHoliday { get; set; }
    public bool SelfPickup { get; set; }
    public bool SelfDelivery { get; set; }
    public bool FreightCollect { get; set; }
    public bool IdentCheck { get; set; }
    public bool ReceiptNotice { get; set; }
    public bool IsNeutralPickup { get; set; }
    public AddressFormModel? NeutralAddress { get; set; }
    public decimal? InsuranceAmount { get; set; }
    public string InsuranceCurrency { get; set; } = "EUR";
    public decimal? CashOnDeliveryAmount { get; set; }
    public string CashOnDeliveryCurrency { get; set; } = "EUR";
    public decimal? ValueOfGoodsAmount { get; set; }
    public string ValueOfGoodsCurrency { get; set; } = "EUR";
    public string? CostCenter { get; set; }
    public string? Dimensions { get; set; }
    public bool HasDelivery { get; set; }
    public DateOnly? DeliveryDate { get; set; }
    public TimeOnly? DeliveryTimeFrom { get; set; }
    public TimeOnly? DeliveryTimeTill { get; set; }
    public string? ShipperRemarks { get; set; }
    public bool ShipperTelephoneAvis { get; set; }
    public string? ConsigneeRemarks { get; set; }
    public bool ConsigneeTelephoneAvis { get; set; }
    public string? ConsigneeDeliveryCode { get; set; }
    public bool ConsigneeDeliveryCodeEncryption { get; set; }

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
            TimeTill = PickupTimeTill,
            AvisFrom = PickupAvisFrom,
            AvisTill = PickupAvisTill,
            WeekendOrHolidayIndicator = PickupWeekendOrHoliday
        },
        Delivery = HasDelivery && DeliveryDate is not null
            ? new TimeWindow
            {
                Date = DeliveryDate.Value,
                TimeFrom = DeliveryTimeFrom,
                TimeTill = DeliveryTimeTill
            }
            : null,
        SelfPickup = SelfPickup,
        SelfDelivery = SelfDelivery,
        FreightCollect = FreightCollect,
        IdentCheck = IdentCheck,
        ReceiptNotice = ReceiptNotice,
        IsNeutralPickup = IsNeutralPickup,
        NeutralAddress = IsNeutralPickup ? NeutralAddress?.ToAddress() : null,
        InsuranceAmount = InsuranceAmount,
        InsuranceCurrency = InsuranceAmount.HasValue ? InsuranceCurrency : null,
        CashOnDeliveryAmount = CashOnDeliveryAmount,
        CashOnDeliveryCurrency = CashOnDeliveryAmount.HasValue ? CashOnDeliveryCurrency : null,
        ValueOfGoodsAmount = ValueOfGoodsAmount,
        ValueOfGoodsCurrency = ValueOfGoodsAmount.HasValue ? ValueOfGoodsCurrency : null,
        CostCenter = CostCenter,
        Dimensions = Dimensions,
        ShipperRemarks = ShipperRemarks,
        ShipperTelephoneAvis = ShipperTelephoneAvis,
        ConsigneeRemarks = ConsigneeRemarks,
        ConsigneeTelephoneAvis = ConsigneeTelephoneAvis,
        ConsigneeDeliveryCode = ConsigneeDeliveryCode,
        ConsigneeDeliveryCodeEncryption = ConsigneeDeliveryCodeEncryption
    };
}
