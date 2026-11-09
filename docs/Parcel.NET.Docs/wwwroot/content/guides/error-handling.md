---
title: Error Handling
category: Guides
order: 1
description: Handle API errors with Parcel.NET's exception hierarchy.
---

## Exception Hierarchy

Parcel.NET uses a structured exception hierarchy rooted in `ParcelException`:

```
ParcelException (base)
├── ShippingException
│   └── DhlShippingValidationException
└── TrackingException
    └── DhlTrackingNotFoundException
```

All exceptions carry carrier-specific error information.

## ParcelException

The base exception provides common error properties:

| Property | Type | Description |
|----------|------|-------------|
| `StatusCode` | `HttpStatusCode?` | The HTTP status code from the carrier API |
| `ErrorCode` | `string?` | Carrier-specific error code |
| `RawResponse` | `string?` | The raw response body for debugging |

`Message` never contains the raw response body: the body can include addresses and other personal data. DHL clients use the `detail` text of the error response if one is present (e.g. `DHL Shipping API returned 400: Invalid billing number.`), otherwise `DHL <API> returned <code> without error details.` The full body is only available via `RawResponse`.

## ShippingException

Thrown when shipping operations fail (creating, cancelling, validating shipments):

```csharp
try
{
    var response = await shippingClient.CreateShipmentAsync(request);
}
catch (ShippingException ex)
{
    Console.WriteLine($"Shipping error: {ex.Message}");
    Console.WriteLine($"HTTP Status: {ex.StatusCode}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");

    // The raw API response may contain personal data (e.g. addresses) —
    // only log it where your data protection rules allow
    if (ex.RawResponse is not null)
    {
        logger.LogDebug("DHL API Response: {Response}", ex.RawResponse);
    }
}
```

## TrackingException

Thrown when tracking operations fail. `StatusCode` holds the HTTP status, `ErrorCode` the carrier-specific error code if the API reports one:

```csharp
try
{
    var result = await trackingClient.TrackAsync("1234567890");
}
catch (TrackingException ex)
{
    Console.WriteLine($"Tracking error: {ex.Message}");
    Console.WriteLine($"HTTP Status: {ex.StatusCode}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
}
```

### DHL Tracking: Shipment Not Found

The DHL Parcel DE Tracking API (`Parcel.NET.Dhl.Tracking`) reports most errors as HTTP 200 with an error code in the XML. When it has no data for a tracking number (code `100`), `DhlTrackingClient` throws `DhlTrackingNotFoundException`, a `TrackingException` that exposes the `TrackingNumber`. Catch it before the general `TrackingException`; other API error codes, such as `5` (login failed) or `62` (missing permission), are thrown as a plain `TrackingException` with the code in `ErrorCode`:

```csharp
try
{
    var result = await dhlTrackingClient.TrackAsync("00340434161094099999");
}
catch (DhlTrackingNotFoundException ex)
{
    Console.WriteLine($"Unknown shipment {ex.TrackingNumber}.");
}
catch (TrackingException ex) when (ex.ErrorCode == "5")
{
    Console.WriteLine("Login failed - check the tracking portal user and password.");
}
catch (TrackingException ex)
{
    Console.WriteLine($"Tracking error: {ex.Message} (code {ex.ErrorCode})");
}
```

See [DHL Tracking](docs/carriers/dhl-tracking) for the full list of error codes.

## Common Error Scenarios

### Invalid Credentials

```csharp
catch (ShippingException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    // API key or OAuth credentials are invalid
}
```

### Rate Limiting

```csharp
catch (ParcelException ex) when (ex.StatusCode == (System.Net.HttpStatusCode)429)
{
    // Too many requests - implement retry with backoff
}
```

### DHL Shipping Validation Errors

When DHL rejects a shipment (HTTP 400/422) with at least one validation error, `CreateShipmentAsync` throws `DhlShippingValidationException`, a `ShippingException` with typed validation messages:

```csharp
try
{
    var response = await shippingClient.CreateShipmentAsync(request);
}
catch (DhlShippingValidationException ex)
{
    // Errors that caused the rejection (never empty)
    foreach (var error in ex.ValidationMessages)
    {
        Console.WriteLine($"{error.Property}: {error.Message}");
    }

    // Additional warnings from the same response
    foreach (var warning in ex.Warnings)
    {
        Console.WriteLine($"Warning - {warning.Property}: {warning.Message}");
    }
}
```

The exception message lists the errors as `property: message`, joined with ` | `. Property paths and texts are passed through as returned by DHL; Parcel.NET adds no values from the request. Responses with warnings only, or without validation messages, throw a plain `ShippingException`.

### Validating Before Creating

For shipping, prefer `ValidateShipmentAsync` to catch errors before creating:

```csharp
var dhlClient = (IDhlShippingClient)shippingClient;
var validation = await dhlClient.ValidateShipmentAsync(request);

if (!validation.Valid)
{
    foreach (var msg in validation.Messages.Where(m => m.Severity == ValidationSeverity.Error))
    {
        Console.WriteLine($"Validation error: {msg.Property} - {msg.Message}");
    }
    return;
}

// Safe to create shipment
var response = await shippingClient.CreateShipmentAsync(request);
```

## Best Practices

1. **Catch specific exceptions** - Use `DhlShippingValidationException`, `ShippingException`, `DhlTrackingNotFoundException` and `TrackingException` instead of the base type
2. **Check StatusCode** - Differentiate between client errors (4xx) and server errors (5xx)
3. **Handle RawResponse with care** - The raw API response contains detailed error information but may include personal data such as addresses; prefer `ValidationMessages` and log `RawResponse` only where your data protection rules allow
4. **Validate first** - Use `ValidateShipmentAsync` before `CreateShipmentAsync`
5. **Handle network errors** - `HttpRequestException` can occur for connectivity issues (separate from `ParcelException`)
