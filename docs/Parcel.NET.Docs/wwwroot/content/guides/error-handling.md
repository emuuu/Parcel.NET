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
└── TrackingException
```

All exceptions carry carrier-specific error information.

## ParcelException

The base exception provides common error properties:

| Property | Type | Description |
|----------|------|-------------|
| `StatusCode` | `HttpStatusCode?` | The HTTP status code from the carrier API |
| `ErrorCode` | `string?` | Carrier-specific error code |
| `RawResponse` | `string?` | The raw response body for debugging |

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

    // Log the raw API response for debugging
    if (ex.RawResponse is not null)
    {
        logger.LogDebug("DHL API Response: {Response}", ex.RawResponse);
    }
}
```

## TrackingException

Thrown when tracking operations fail:

```csharp
try
{
    var result = await trackingClient.TrackAsync("1234567890");
}
catch (TrackingException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    Console.WriteLine("Tracking number not found.");
}
catch (TrackingException ex)
{
    Console.WriteLine($"Tracking error: {ex.Message}");
}
```

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

### Validation Errors

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

1. **Catch specific exceptions** - Use `ShippingException` and `TrackingException` instead of the base type
2. **Check StatusCode** - Differentiate between client errors (4xx) and server errors (5xx)
3. **Log RawResponse** - The raw API response contains detailed error information
4. **Validate first** - Use `ValidateShipmentAsync` before `CreateShipmentAsync`
5. **Handle network errors** - `HttpRequestException` can occur for connectivity issues (separate from `ParcelException`)
