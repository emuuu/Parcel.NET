using System.Net;
using System.Text;

namespace Parcel.NET.Dhl.EPost.Tests;

/// <summary>
/// Test message handler that returns a queued sequence of responses and records every request
/// (method, URI, Authorization header and body). Once the queue is exhausted the last responder repeats.
/// </summary>
internal sealed class RecordingHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responders = new();
    private Func<HttpRequestMessage, HttpResponseMessage> _last = _ => new HttpResponseMessage(HttpStatusCode.OK);

    public List<HttpMethod> Methods { get; } = [];
    public List<string> Urls { get; } = [];
    public List<string?> AuthParameters { get; } = [];
    public List<string> Bodies { get; } = [];

    public int CallCount => Methods.Count;

    public RecordingHandler Enqueue(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responders.Enqueue(responder);
        _last = responder;
        return this;
    }

    public RecordingHandler Enqueue(HttpStatusCode status, string? json = null) =>
        Enqueue(_ => Json(status, json));

    public static HttpResponseMessage Json(HttpStatusCode status, string? json) =>
        new(status)
        {
            Content = json is null
                ? null
                : new StringContent(json, Encoding.UTF8, "application/json")
        };

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Methods.Add(request.Method);
        Urls.Add(request.RequestUri?.ToString() ?? string.Empty);
        AuthParameters.Add(request.Headers.Authorization?.Parameter);
        Bodies.Add(request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

        var responder = _responders.Count > 0 ? _responders.Dequeue() : _last;
        return responder(request);
    }
}
