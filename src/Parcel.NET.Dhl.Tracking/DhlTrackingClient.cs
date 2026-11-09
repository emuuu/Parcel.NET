using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Parcel.NET.Dhl.Internal;
using Parcel.NET.Dhl.Tracking.Internal;
using Parcel.NET.Dhl.Tracking.Models;

namespace Parcel.NET.Dhl.Tracking;

/// <summary>
/// DHL Parcel DE Tracking API v0 client (XML) implementing <see cref="IDhlTrackingClient"/>.
/// </summary>
/// <remarks>
/// <para>
/// Authentication has two parts, both required for the sandbox and production environments: the API gateway app
/// (<see cref="DhlOptions.ApiKey"/> and <see cref="DhlOptions.ApiSecret"/>, sent as <c>Authorization: Basic</c>
/// by this client on every request, plus the <c>dhl-api-key</c> header added by <see cref="DhlApiKeyHandler"/>),
/// and the business customer portal user (<see cref="DhlOptions.TrackingUsername"/> and
/// <see cref="DhlOptions.TrackingPassword"/>, sent as <c>appname</c>/<c>password</c> inside the XML query).
/// </para>
/// <para>
/// The XML query, including <c>appname</c> and <c>password</c>, is part of the request URL
/// (<c>?xml=...</c>); this is how the DHL API is designed. The default HttpClientFactory logging redacts the query
/// string (<c>?*</c>); do not add handlers, tracing or proxies that record the full request URI.
/// </para>
/// <para>
/// Timestamps in responses (<c>dd.MM.yyyy HH:mm</c>) carry no time zone. DHL does not document the zone;
/// they are interpreted as German local time (Europe/Berlin).
/// </para>
/// </remarks>
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
    /// <exception cref="DhlTrackingNotFoundException">The API returned code <c>100</c> (no data found for the tracking number).</exception>
    /// <exception cref="TrackingException">The request failed, or the API returned any other error code (for example <c>5</c>, login failed).</exception>
    public async Task<TrackingResult> TrackAsync(
        string trackingNumber,
        DhlTrackingOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        var xml = BuildXmlQuery("d-get-piece-detail", trackingNumber, options);
        return await ExecuteTrackingRequestAsync(trackingNumber, xml, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <exception cref="DhlTrackingNotFoundException">The API returned code <c>100</c> (no data found for the tracking number).</exception>
    /// <exception cref="TrackingException">The request failed, or the API returned any other error code (for example <c>5</c>, login failed).</exception>
    public async Task<TrackingResult> TrackPublicAsync(
        string trackingNumber,
        DhlTrackingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        var xml = BuildXmlQuery("get-status-for-public-user", trackingNumber, options);
        return await ExecuteTrackingRequestAsync(trackingNumber, xml, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <exception cref="TrackingException">The request failed, or the API returned an error code other than <c>100</c> (for example <c>5</c>, login failed, or <c>63</c>, signature archive unavailable).</exception>
    public async Task<byte[]?> GetSignatureAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        var xml = BuildXmlQuery("d-get-signature", trackingNumber, null);
        var responseXml = await SendAsync(xml, cancellationToken).ConfigureAwait(false);

        var errorCode = DhlTrackingXmlParser.GetErrorCode(responseXml);
        if (errorCode == 100)
            return null;

        if (errorCode != 0)
            throw CreateApiException(trackingNumber, errorCode, responseXml);

        var imageHex = DhlTrackingXmlParser.ParseSignatureResponse(responseXml);
        return imageHex is not null ? Convert.FromHexString(imageHex) : null;
    }

    private async Task<TrackingResult> ExecuteTrackingRequestAsync(
        string trackingNumber,
        string xmlQuery,
        CancellationToken cancellationToken)
    {
        var responseBody = await SendAsync(xmlQuery, cancellationToken).ConfigureAwait(false);

        var errorCode = DhlTrackingXmlParser.GetErrorCode(responseBody);
        if (errorCode != 0)
            throw CreateApiException(trackingNumber, errorCode, responseBody);

        return DhlTrackingXmlParser.ParseTrackingResponse(responseBody);
    }

    /// <summary>
    /// Sends the XML query and returns the response body. A non-success HTTP status throws a <see cref="TrackingException"/>.
    /// </summary>
    private async Task<string> SendAsync(string xmlQuery, CancellationToken cancellationToken)
    {
        // Basic authentication is set per request so that it applies regardless of how the HttpClient was created.
        using var request = new HttpRequestMessage(HttpMethod.Get, $"?xml={Uri.EscapeDataString(xmlQuery)}");
        request.Headers.Authorization = CreateBasicAuthorization();

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new TrackingException(
                DhlErrorHelper.FormatErrorMessage("DHL Tracking API", response.StatusCode, body),
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                body);
        }

        return body;
    }

    private static TrackingException CreateApiException(string trackingNumber, int errorCode, string responseBody)
    {
        var errorMessage = DhlTrackingXmlParser.GetErrorMessage(responseBody) ?? "Unknown error";
        var message = $"DHL Tracking API error {errorCode}: {errorMessage}";

        if (errorCode == 100)
            return new DhlTrackingNotFoundException(trackingNumber, message, responseBody);

        return new TrackingException(
            message,
            statusCode: null,
            errorCode: errorCode.ToString(),
            rawResponse: responseBody);
    }

    private AuthenticationHeaderValue CreateBasicAuthorization()
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
            throw new InvalidOperationException("DHL ApiKey is required for Parcel DE Tracking.");
        if (string.IsNullOrEmpty(_options.ApiSecret))
            throw new InvalidOperationException("DHL ApiSecret is required for Parcel DE Tracking (Basic authentication with ApiKey:ApiSecret).");

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ApiKey}:{_options.ApiSecret}"));
        return new AuthenticationHeaderValue("Basic", credentials);
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
