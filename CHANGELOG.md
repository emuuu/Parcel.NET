# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Initial project structure with carrier-agnostic abstractions
- `IShipmentService` and `ITrackingService` interfaces
- DHL Parcel DE Shipping API v2 client (`Parcel.NET.Dhl.Shipping`)
- DHL Shipment Tracking API v1 client (`Parcel.NET.Dhl.Tracking`)
- OAuth token management with caching (`DhlTokenService`)
- Custom exception hierarchy (`ParcelException`, `ShippingException`, `TrackingException`)
- Dependency injection extensions with builder pattern
- JSON source generators for AOT compatibility
- Central package management (`Directory.Packages.props`)
- SourceLink and symbol package support
- DocFX configuration for API documentation generation
- Console sample project
- `DhlShippingValidationException` (`Parcel.NET.Dhl.Shipping`): thrown by `DhlShippingClient` on HTTP 400/422 when the response contains at least one `items[].validationMessages[]` entry with state `Error`. Exposes `ValidationMessages` (the errors, never empty) and `Warnings` as typed `ValidationMessage` lists. The exception message appends the affected property paths and message texts as returned by DHL (`property: message`, joined with ` | `); Parcel.NET adds no request values, but cannot guarantee that DHL's texts never quote input values. Responses with warnings only, or without validation messages, still throw `ShippingException`.

### Changed

- **Behavior change:** `LetterXpressClient` rejects registered mail unless `LetterSpecification.Shipping` is `ShippingType.National`, throwing `ArgumentException` before any request is sent. LetterXpress rejects registered mail not only for `International` but also for `Auto` (the default), so such requests previously failed only at the API. The shipping type is never switched implicitly, since `National` would be wrong for foreign addresses. `UpdatePrintJobAsync` applies the same check when an update sets both `Registered` and `Shipping`.
- **Behavior change:** error messages of all DHL clients (Shipping, Tracking, Unified Tracking, Internetmarke, Location Finder, Pickup, Returns) no longer contain the raw response body, which may include addresses and other personal data. If the body carries no `status.detail`/`detail`, the message now reads `DHL <API> returned <code> without error details.` The raw body remains available via `ParcelException.RawResponse`; callers that parsed `Exception.Message` should use `RawResponse` or `DhlShippingValidationException` instead.

### Fixed

- `DhlShippingClient.ValidateShipmentAsync` no longer fails on unexpected `validationState` values (e.g. non-string types); `Error` is now matched case-insensitively and any other or missing state is reported as `Warning`, consistent with `DhlShippingValidationException`.
- `DhlShippingClient.ValidateShipmentAsync` throws `ShippingException` (with status code and raw response) instead of `JsonException` when an error response is not valid JSON.
- `LetterXpressClient` throws `LetterXpressException` (with HTTP status code and raw response) instead of `InvalidOperationException` when LetterXpress returns a validation error whose `message` is an object (`{"message":{"letter.registered":["..."]}}`). Such messages are flattened to `field: message`, multiple entries joined with `; `; array-valued messages are joined the same way. Parsing LetterXpress error bodies never throws and falls back to the raw body.
- `LetterXpressClient` no longer reports `Failed to deserialize LetterXpress response.` (losing the status code) when an HTTP 200 response carries an application-level error with an object-valued `message`.
- Parsing DHL error bodies never throws, including for bodies with undecodable strings (e.g. unpaired surrogate escapes).

[Unreleased]: https://github.com/Parcel.NET/Parcel.NET/commits/main
