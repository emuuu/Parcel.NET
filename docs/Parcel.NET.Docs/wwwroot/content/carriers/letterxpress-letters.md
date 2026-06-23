---
title: LetterXpress Letters
category: Carriers
subcategory: LetterXpress
order: 1
description: Send physical letters and SMART@MAIL e-mails, query prices and balance, and manage print jobs with the LetterXpress (A&O Fischer) API v3.
apiRef: ILetterXpressClient
---

## Overview

`LetterXpressClient` implements the [LetterXpress](https://www.letterxpress.de/) (A&O Fischer GmbH & Co. KG) API v3 — a hybrid print-and-mail service: you upload a PDF, LetterXpress prints it and sends it as a physical letter, optionally combined with the **SMART@MAIL** e-mail product.

Because LetterXpress is a letter/print-mail product rather than a parcel carrier, it does **not** implement the carrier-agnostic `IShipmentService`/`ITrackingService` abstractions. It exposes its own `ILetterXpressClient` interface, mirroring the DHL Internetmarke design.

Supported operations:

- **Account** — query balance and price
- **Print jobs** — create, retrieve (incl. rendered PDF), update, delete, and list
- **E-mail jobs (SMART@MAIL)** — create (`maildirect` / `mailplus` / `mailsecure`), retrieve, update, delete, and list
- **Accounting** — list transactions and invoices (incl. invoice PDF)

## Installation

```bash
dotnet add package Parcel.NET.LetterXpress.Letters
# or the meta-package
dotnet add package Parcel.NET.LetterXpress.All
```

## Registration

```csharp
builder.Services.AddLetterXpress(options =>
{
    options.Username = "your-username";   // Mein Konto > Zugangsdaten > LXP API
    options.ApiKey = "your-api-key";
    options.UseTestMode = true;           // true => Postbox, no real processing
})
.AddLetterXpressLetters();
```

## Authentication & modes

Authentication is sent in the JSON **body** of every request (an `auth` object with `username`, `apikey`, and `mode`) — there is no auth header. The client handles this for you.

- **Test mode** (`UseTestMode = true`): jobs are placed in the **Postbox** instead of being processed (and therefore not dispatched). Postbox jobs can be reviewed, deleted, or sent, and are deleted automatically after 7 days.
- **Live mode** (`UseTestMode = false`): jobs are processed and dispatched directly.

> LetterXpress is a server-to-server API and intentionally does **not** enable CORS. Use it from your backend, never directly from a browser — that would expose your API key.

## Examples

### Balance and price

```csharp
var client = serviceProvider.GetRequiredService<ILetterXpressClient>();

Balance balance = await client.GetBalanceAsync();
Console.WriteLine($"{balance.Amount} {balance.Currency}");

PriceResult price = await client.GetPriceAsync(new PriceRequest
{
    Pages = 1,
    Color = LetterColor.BlackWhite,
    Mode = PrintMode.Simplex,
    Shipping = ShippingType.National   // price supports only National or International
});
```

### Create a print job

The PDF is supplied as raw bytes; the client computes the base64 encoding and MD5 checksum automatically.

```csharp
PrintJob job = await client.CreatePrintJobAsync(new LetterRequest
{
    File = await File.ReadAllBytesAsync("invoice.pdf"),
    FilenameOriginal = "invoice.pdf",
    Specification = new LetterSpecification
    {
        Color = LetterColor.Color,
        Mode = PrintMode.Duplex,
        Shipping = ShippingType.National
    },
    Registered = RegisteredMail.Einschreiben   // national only
});

// Within 15 minutes you can still update or delete it:
await client.UpdatePrintJobAsync(job.Id, new PrintJobUpdate { Notice = "Customer 4711" });
await client.DeletePrintJobAsync(job.Id);
```

### SMART@MAIL e-mail job

```csharp
EmailJobResult result = await client.CreateEmailJobAsync(new LetterRequest
{
    File = await File.ReadAllBytesAsync("invoice.pdf"),
    Specification = new LetterSpecification { Shipping = ShippingType.National },
    EmailLetter = new EmailLetterOptions
    {
        EmailOption = EmailOption.MailDirect,
        EmailReceiver = "customer@example.com"
    }
});

foreach (var email in result.EmailJobs)
    Console.WriteLine($"{email.Id}: {email.EmailReceiver} ({email.Status})");
```

### Listing and filtering

```csharp
PagedResult<PrintJob> queued = await client.ListPrintJobsAsync(PrintJobFilter.Queue);
PagedResult<EmailJob> sent   = await client.ListEmailJobsAsync(EmailJobFilter.Success);
PagedResult<Transaction> tx  = await client.ListTransactionsAsync(TransactionFilter.PrintJobs);
PagedResult<Invoice> invoices = await client.ListInvoicesAsync();
```

## Notes & limits

- **Limits:** max. 50 MB per request (validated locally) and 120 requests/minute. Throttling/retry on HTTP 429 is left to your `HttpClient` pipeline (e.g. a `Microsoft.Extensions.Http.Resilience` handler).
- **Timestamps:** the API returns timestamps without a timezone designator, in German local time (Europe/Berlin, CET/CEST). The client parses them into `DateTimeOffset` with the correct (DST-aware) offset.
- **Constraints enforced by the client:** registered mail is national-only; a serial letter cannot be combined with a single `EmailLetter`; price requests accept only `National`/`International` shipping.
- **Errors** surface as `LetterXpressException` carrying the HTTP/body status code and the raw response.
