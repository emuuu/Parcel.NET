namespace ParcelNET.GoExpress.Shipping.Models;

/// <summary>
/// Label output formats supported by the GO! Express API.
/// </summary>
public enum GoExpressLabelFormat
{
    /// <summary>ZPL format (API value "1").</summary>
    Zpl,
    /// <summary>PDF 4x6 inch format (API value "2").</summary>
    Pdf4x6,
    /// <summary>PDF A4 format (API value "4").</summary>
    PdfA4,
    /// <summary>TPCL format (API value "5").</summary>
    Tpcl
}
