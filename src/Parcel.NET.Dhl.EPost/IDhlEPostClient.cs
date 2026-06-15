using Parcel.NET.Dhl.EPost.Models;

namespace Parcel.NET.Dhl.EPost;

/// <summary>
/// Client for the Deutsche Post E-POSTBUSINESS API v2 (hybrid mail / E-POST MAILER product):
/// submit PDF documents that are printed and physically delivered as letters.
/// </summary>
public interface IDhlEPostClient
{
    /// <summary>
    /// Submits one or more letters for hybrid-mail delivery (<c>POST /api/Letter</c>).
    /// </summary>
    /// <param name="letters">The letters to submit.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>One result per letter, mapping file name to the assigned shipment id.</returns>
    Task<IReadOnlyList<EPostLetterResult>> SubmitLettersAsync(
        IEnumerable<EPostLetterRequest> letters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the status of a single shipment (<c>GET /api/Letter/{letterID}</c>).
    /// </summary>
    /// <param name="letterId">The shipment id returned by <see cref="SubmitLettersAsync"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The shipment status.</returns>
    Task<EPostLetterStatus> GetLetterStatusAsync(long letterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the status of multiple shipments in one call (<c>POST /api/Letter/StatusQuery</c>).
    /// Prefer this over polling individual ids to stay within the API rate limits.
    /// </summary>
    /// <param name="letterIds">The shipment ids to query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The statuses of the requested shipments.</returns>
    Task<IReadOnlyList<EPostLetterStatus>> GetLetterStatusesAsync(
        IEnumerable<long> letterIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks API availability (<c>GET /api/Login/HealthCheck</c>). Requires no authentication.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the API responds successfully.</returns>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}
