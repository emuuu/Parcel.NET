using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Parcel.NET.Dhl.Tracking.Internal;
using Parcel.NET.Dhl.Tracking.Models;

namespace Parcel.NET.Dhl.Tracking;

/// <summary>
/// DHL Parcel DE Tracking API v0 client (XML) implementing <see cref="IDhlTrackingClient"/>.
/// Uses XML query parameters with appname/password credentials.
/// </summary>
public class DhlTrackingClient : IDhlTrackingClient
{
    private readonly HttpClient _httpClient;
    private readonly DhlOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="DhlTrackingClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for DHL Parcel DE Tracking API requests.</param>
    /// <param name="options">DHL configuration options containing tracking credentials.</param>
    public DhlTrackingClient(HttpClient httpClient, IOptions<DhlOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options.Value;
    }

    /// <inheritdoc />
    public Task<TrackingResult> TrackAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default)
        => TrackAsync(trackingNumber, null, cancellationToken);

    /// <inheritdoc />
    public async Task<TrackingResult> TrackAsync(
        string trackingNumber,
        DhlTrackingOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        var xml = BuildXmlQuery("d-get-piece-detail", trackingNumber, options);
        return await ExecuteTrackingRequestAsync(xml, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TrackingResult> TrackPublicAsync(
        string trackingNumber,
        DhlTrackingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        var xml = BuildXmlQuery("get-status-for-public-user", trackingNumber, options);
        return await ExecuteTrackingRequestAsync(xml, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<byte[]?> GetSignatureAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        var xml = BuildXmlQuery("d-get-signature", trackingNumber, null);
        var url = $"?xml={Uri.EscapeDataString(xml)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new TrackingException(
                $"DHL Tracking API returned {(int)response.StatusCode}: {rawBody}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var responseXml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var errorCode = DhlTrackingXmlParser.GetErrorCode(responseXml);
        if (errorCode != 0)
            return null;

        var imageBase64 = DhlTrackingXmlParser.ParseSignatureResponse(responseXml);
        return imageBase64 is not null ? Convert.FromBase64String(imageBase64) : null;
    }

    private async Task<TrackingResult> ExecuteTrackingRequestAsync(string xmlQuery, CancellationToken cancellationToken)
    {
        var url = $"?xml={Uri.EscapeDataString(xmlQuery)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new TrackingException(
                $"DHL Tracking API returned {(int)response.StatusCode}: {responseBody}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                responseBody);
        }

        var errorCode = DhlTrackingXmlParser.GetErrorCode(responseBody);
        if (errorCode != 0)
        {
            var errorMessage = DhlTrackingXmlParser.GetErrorMessage(responseBody) ?? "Unknown error";
            throw new TrackingException(
                $"DHL Tracking API error {errorCode}: {errorMessage}",
                statusCode: null,
                errorCode: errorCode.ToString(),
                rawResponse: responseBody);
        }

        return DhlTrackingXmlParser.ParseTrackingResponse(responseBody);
    }

    internal string BuildXmlQuery(string requestType, string trackingNumber, DhlTrackingOptions? options)
    {
        var appname = _options.TrackingUsername
            ?? throw new InvalidOperationException("DHL TrackingUsername is required for Parcel DE Tracking.");
        var password = _options.TrackingPassword
            ?? throw new InvalidOperationException("DHL TrackingPassword is required for Parcel DE Tracking.");

        var language = options?.Language ?? "de";
        var zipCode = options?.ZipCode;

        var element = new XElement("data",
            new XAttribute("appname", appname),
            new XAttribute("password", password),
            new XAttribute("language-code", language),
            new XAttribute("request", requestType),
            new XAttribute("piece-code", trackingNumber));

        if (zipCode is not null)
            element.Add(new XAttribute("zip-code", zipCode));

        return element.ToString(SaveOptions.DisableFormatting);
    }
}
