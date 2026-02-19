using ParcelNET.Abstractions;
using ParcelNET.Abstractions.Models;
using ParcelNET.GoExpress.Shipping.Models;

namespace ParcelNET.GoExpress.Shipping;

/// <summary>
/// GO! Express-specific shipping client extending the carrier-agnostic <see cref="IShipmentService"/>
/// with additional GO! Express operations.
/// </summary>
public interface IGoExpressShippingClient : IShipmentService
{
    /// <summary>
    /// Generates a label for an existing shipment identified by its HWB number.
    /// </summary>
    /// <param name="hwbNumber">The HWB (Hauptweg-Brief) number of the shipment.</param>
    /// <param name="format">The desired label format. Defaults to <see cref="GoExpressLabelFormat.PdfA4"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The generated label.</returns>
    Task<ShipmentLabel> GenerateLabelAsync(string hwbNumber, GoExpressLabelFormat format = GoExpressLabelFormat.PdfA4, CancellationToken cancellationToken = default);
}
