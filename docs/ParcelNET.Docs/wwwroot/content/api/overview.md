---
title: API Reference
category: API Reference
order: 1
description: Overview of ParcelNET namespaces and types.
---

## Namespaces

ParcelNET is organized into the following namespaces:

### ParcelNET.Abstractions

Core interfaces and shared models used by all carrier implementations.

| Type | Kind | Description |
|------|------|-------------|
| `IShipmentService` | Interface | Carrier-agnostic shipment operations |
| `ITrackingService` | Interface | Carrier-agnostic tracking operations |
| `Address` | Model | Postal address |
| `ContactInfo` | Model | Contact name, email, phone |
| `Package` | Model | Package weight and dimensions |
| `Dimensions` | Model | Length, width, height |
| `ShipmentRequest` | Model | Base shipment creation request |
| `ShipmentResponse` | Model | Shipment creation response with labels |
| `ShipmentLabel` | Model | Label content and format |
| `TrackingResult` | Model | Tracking status and events |
| `TrackingEvent` | Model | Single tracking event |
| `TrackingStatus` | Enum | Carrier-agnostic delivery status |
| `CancellationResult` | Model | Shipment cancellation result |

### ParcelNET.Abstractions.Exceptions

| Type | Kind | Description |
|------|------|-------------|
| `ParcelNetException` | Exception | Base exception with StatusCode, ErrorCode, RawResponse |
| `ShippingException` | Exception | Thrown on shipping API failures |
| `TrackingException` | Exception | Thrown on tracking API failures |

### ParcelNET.Dhl

DHL core services and authentication.

| Type | Kind | Description |
|------|------|-------------|
| `DhlOptions` | Options | API key, credentials, sandbox toggle |
| `DhlBuilder` | Builder | Fluent builder for registering DHL sub-services |
| `DhlServiceCollectionExtensions` | Static | `AddDhl()` extension method |
| `IDhlTokenService` | Interface | OAuth token management |

### ParcelNET.Dhl.Shipping

DHL Parcel DE Shipping API v2 client.

| Type | Kind | Description |
|------|------|-------------|
| `IDhlShippingClient` | Interface | DHL-specific shipping operations |
| `DhlShippingClient` | Client | Shipping API implementation |
| `DhlShipmentRequest` | Model | DHL-specific shipment request |
| `DhlConsignee` | Model | Alternative delivery (Packstation, post office) |
| `DhlLabelOptions` | Model | Label format and size options |
| `DhlValueAddedServices` | Model | Preferred day, insurance, etc. |
| `DhlProduct` | Enum | DHL product codes (V01PAK, etc.) |
| `ValidationResult` | Model | Shipment validation result |
| `ManifestResult` | Model | Daily manifest result |

### ParcelNET.Dhl.Tracking

DHL Shipment Tracking API v1 client.

| Type | Kind | Description |
|------|------|-------------|
| `DhlTrackingClient` | Client | Tracking API implementation |
| `DhlTrackingOptions` | Model | Language, postal code, country filter |

## Auto-Generated API Docs

The tables on individual documentation pages (marked with "API Reference") are automatically generated from the library's XML documentation comments using reflection. They stay in sync with the source code.
