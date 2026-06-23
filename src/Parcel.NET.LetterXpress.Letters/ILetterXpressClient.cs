using Parcel.NET.LetterXpress.Letters.Models;

namespace Parcel.NET.LetterXpress.Letters;

/// <summary>
/// Client for the LetterXpress (A&amp;O Fischer) API v3.
/// </summary>
public interface ILetterXpressClient
{
    /// <summary>
    /// Retrieves the account balance (GET /v3/balance).
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The account balance.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<Balance> GetBalanceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the price for a letter (GET /v3/price).
    /// </summary>
    /// <param name="request">The price request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The price result.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<PriceResult> GetPriceAsync(PriceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists print jobs (GET /v3/printjobs), ordered from newest to oldest.
    /// </summary>
    /// <param name="filter">Optional status filter.</param>
    /// <param name="page">Optional page number.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A paged list of print jobs.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<PagedResult<PrintJob>> ListPrintJobsAsync(PrintJobFilter? filter = null, int? page = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits a print job (POST /v3/printjobs).
    /// </summary>
    /// <param name="request">The letter to submit.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created print job.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<PrintJob> CreatePrintJobAsync(LetterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single print job, including the rendered PDF (GET /v3/printjobs/{id}).
    /// </summary>
    /// <param name="id">The print job ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The print job.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<PrintJob> GetPrintJobAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes a print job (PUT /v3/printjobs/{id}). Only possible within the first 15 minutes
    /// after submission, and the PDF itself cannot be changed.
    /// </summary>
    /// <param name="id">The print job ID.</param>
    /// <param name="update">The changes to apply.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated print job.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<PrintJob> UpdatePrintJobAsync(long id, PrintJobUpdate update, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a print job (DELETE /v3/printjobs/{id}). Only possible within the first 15 minutes
    /// after submission; jobs with status <c>done</c> cannot be deleted.
    /// </summary>
    /// <param name="id">The print job ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task DeletePrintJobAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits an e-mail job (POST /v3/emailjobs) using LXP SMART@MAIL.
    /// </summary>
    /// <param name="request">The letter to submit. Provide <see cref="LetterRequest.EmailLetter"/> for a single
    /// job, or omit it and use a white code in the PDF for serial processing.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created e-mail job(s) and any associated print jobs.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<EmailJobResult> CreateEmailJobAsync(LetterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single e-mail job (GET /v3/emailjobs/{id}).
    /// </summary>
    /// <param name="id">The e-mail job ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The e-mail job.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<EmailJob> GetEmailJobAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes an e-mail job's recipient (PUT /v3/emailjobs/{id}). Only possible within the first
    /// 15 minutes after submission.
    /// </summary>
    /// <param name="id">The e-mail job ID.</param>
    /// <param name="emailReceiver">The new recipient e-mail address.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated e-mail job.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<EmailJob> UpdateEmailJobAsync(long id, string emailReceiver, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an e-mail job (DELETE /v3/emailjobs/{id}). Only possible within the first 15 minutes
    /// after submission; jobs with status <c>success</c> cannot be deleted.
    /// </summary>
    /// <param name="id">The e-mail job ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task DeleteEmailJobAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists e-mail jobs (GET /v3/emailjobs).
    /// </summary>
    /// <param name="filter">Optional status filter.</param>
    /// <param name="page">Optional page number.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A paged list of e-mail jobs.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<PagedResult<EmailJob>> ListEmailJobsAsync(EmailJobFilter? filter = null, int? page = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists account transactions (GET /v3/transactions).
    /// </summary>
    /// <param name="filter">Optional transaction filter.</param>
    /// <param name="page">Optional page number.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A paged list of transactions.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<PagedResult<Transaction>> ListTransactionsAsync(TransactionFilter? filter = null, int? page = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists invoices (GET /v3/invoices).
    /// </summary>
    /// <param name="page">Optional page number.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A paged list of invoices.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<PagedResult<Invoice>> ListInvoicesAsync(int? page = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single invoice, including the PDF (GET /v3/invoices/{id}).
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The invoice.</returns>
    /// <exception cref="LetterXpressException">Thrown when the API returns an error.</exception>
    Task<Invoice> GetInvoiceAsync(long id, CancellationToken cancellationToken = default);
}
