using Parcel.NET.Dhl.Internetmarke.Models;

namespace Parcel.NET.Docs.Playground.Models;

public class DhlInternetmarkeCheckoutFormModel
{
    public int ProductCode { get; set; } = 10001;
    public int Total { get; set; } = 85;
    public int PageFormatId { get; set; } = 1;
    public string Dpi { get; set; } = "DPI300";
    public string VoucherLayout { get; set; } = "ADDRESS_ZONE";
    public string Format { get; set; } = "PDF";

    // Sender
    public string SenderName { get; set; } = "Sender GmbH";
    public string? SenderAddress { get; set; } = "Senderstrasse 1";
    public string SenderPostalCode { get; set; } = "10115";
    public string SenderCity { get; set; } = "Berlin";

    // Receiver
    public string ReceiverName { get; set; } = "Max Mustermann";
    public string? ReceiverAddress { get; set; } = "Empfaengerstrasse 42";
    public string ReceiverPostalCode { get; set; } = "80331";
    public string ReceiverCity { get; set; } = "Munich";

    public CheckoutRequest ToRequest(string shopOrderId) => new()
    {
        ShopOrderId = shopOrderId,
        Total = Total,
        PageFormatId = PageFormatId,
        Dpi = Dpi,
        Positions =
        [
            new CheckoutPosition
            {
                ProductCode = ProductCode,
                VoucherLayout = VoucherLayout,
                Sender = new InternetmarkeAddress
                {
                    Name = SenderName,
                    AddressLine1 = string.IsNullOrWhiteSpace(SenderAddress) ? null : SenderAddress,
                    PostalCode = SenderPostalCode,
                    City = SenderCity
                },
                Receiver = new InternetmarkeAddress
                {
                    Name = ReceiverName,
                    AddressLine1 = string.IsNullOrWhiteSpace(ReceiverAddress) ? null : ReceiverAddress,
                    PostalCode = ReceiverPostalCode,
                    City = ReceiverCity
                }
            }
        ]
    };
}
