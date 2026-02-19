using System.Diagnostics;
using System.Net;
using Parcel.NET.Abstractions.Exceptions;

namespace Parcel.NET.Docs.Playground.Models;

public class PlaygroundResult
{
    public bool IsSuccess { get; init; }
    public object? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? RawResponse { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public TimeSpan Duration { get; init; }

    public static PlaygroundResult Success(object data, TimeSpan duration) => new()
    {
        IsSuccess = true,
        Data = data,
        Duration = duration
    };

    public static PlaygroundResult Error(Exception ex, TimeSpan duration) => new()
    {
        IsSuccess = false,
        ErrorMessage = ex.Message,
        RawResponse = ex is ParcelNetException pne ? pne.RawResponse : null,
        StatusCode = ex is ParcelNetException pne2 ? pne2.StatusCode : null,
        Duration = duration
    };
}
