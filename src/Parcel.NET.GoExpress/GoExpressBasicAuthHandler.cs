using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace Parcel.NET.GoExpress;

/// <summary>
/// HTTP message handler that adds Basic Authentication headers for GO! Express API requests.
/// </summary>
public class GoExpressBasicAuthHandler : DelegatingHandler
{
    private readonly string _credentials;

    /// <summary>
    /// Initializes a new instance of <see cref="GoExpressBasicAuthHandler"/>.
    /// </summary>
    /// <param name="options">GO! Express configuration options.</param>
    public GoExpressBasicAuthHandler(IOptions<GoExpressOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var opts = options.Value;
        _credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{opts.Username}:{opts.Password}"));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
