# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Packages are versioned per family (Abstractions, DHL, GoExpress, LetterXpress and the `Parcel.NET.All` meta-package). Each release section names its family and lists the affected packages.

## [Unreleased]

### Changed

- Updated dependencies of the DHL packages (`Microsoft.Extensions.*` 10.0.14).

## [All 1.1.0] - 2026-11-05

Packages: Parcel.NET.All

### Added

- `Parcel.NET.All` now includes the LetterXpress packages (`Parcel.NET.LetterXpress.All`) in addition to DHL and GoExpress. They were added to the project after All 1.0.2 and were not part of that release.

### Changed

- References the 1.1.0 releases of the DHL, GoExpress and LetterXpress families.
- Package description and README state that the project is unofficial.

## [GoExpress 1.1.0] - 2026-11-05

Packages: Parcel.NET.GoExpress, Parcel.NET.GoExpress.Shipping, Parcel.NET.GoExpress.Tracking, Parcel.NET.GoExpress.All

### Changed

- No functional changes. The version is raised to align with the other package families (DHL, LetterXpress and `Parcel.NET.All` at 1.1.0).
- Package descriptions and READMEs state that the project is unofficial and not affiliated with GO! Express & Logistics.
- Updated dependencies.

## [LetterXpress 1.1.0] - 2026-11-05

Packages: Parcel.NET.LetterXpress, Parcel.NET.LetterXpress.Letters, Parcel.NET.LetterXpress.All

### Changed

- Updated dependencies.

### Fixed

- `LetterXpressClient` streams print and e-mail job requests (`CreatePrintJobAsync`, `CreateEmailJobAsync`) into the HTTP request instead of building the whole payload in memory: PDFs, attachments, backgrounds and terms are no longer converted to intermediate Base64 strings, each one is encoded directly into the output buffer while the request is written, and the `base64_file_checksum` is computed block-wise. Peak memory per request now depends on the Base64 size of the largest single PDF rather than on several copies of the entire payload. The JSON content has the same meaning as before and `Content-Type` stays `application/json`; these requests are now sent with an exact `Content-Length` instead of chunked transfer encoding. No public API changed.

## [LetterXpress 1.0.1] - 2026-10-23

Packages: Parcel.NET.LetterXpress, Parcel.NET.LetterXpress.Letters, Parcel.NET.LetterXpress.All

### Changed

- **Behavior change:** `LetterXpressClient` rejects registered mail unless `LetterSpecification.Shipping` is `ShippingType.National`, throwing `ArgumentException` before any request is sent. LetterXpress rejects registered mail not only for `International` but also for `Auto` (the default), so such requests previously failed only at the API. The shipping type is never switched implicitly, since `National` would be wrong for foreign addresses. `UpdatePrintJobAsync` applies the same check when an update sets both `Registered` and `Shipping`.
- Package descriptions state that the project is unofficial and not affiliated with A&O Fischer (LetterXpress).
- Updated dependencies.

### Fixed

- `LetterXpressClient` throws `LetterXpressException` (with HTTP status code and raw response) instead of `InvalidOperationException` when LetterXpress returns a validation error whose `message` is an object (`{"message":{"letter.registered":["..."]}}`). Such messages are flattened to `field: message`, multiple entries joined with `; `; array-valued messages are joined the same way. Parsing LetterXpress error bodies never throws and falls back to the raw body.
- `LetterXpressClient` no longer reports `Failed to deserialize LetterXpress response.` (losing the status code) when an HTTP 200 response carries an application-level error with an object-valued `message`.

## [Abstractions 1.1.0] - 2026-10-19

Packages: Parcel.NET.Abstractions

### Changed

- Package description and README state that the project is unofficial. No functional changes; released alongside DHL 1.1.0.

## [DHL 1.1.0] - 2026-10-19

Packages: Parcel.NET.Dhl, Parcel.NET.Dhl.Shipping, Parcel.NET.Dhl.Tracking, Parcel.NET.Dhl.UnifiedTracking, Parcel.NET.Dhl.Pickup, Parcel.NET.Dhl.Returns, Parcel.NET.Dhl.Internetmarke, Parcel.NET.Dhl.LocationFinder, Parcel.NET.Dhl.All

### Added

- `DhlShippingValidationException` (`Parcel.NET.Dhl.Shipping`): thrown by `DhlShippingClient` on HTTP 400/422 when the response contains at least one `items[].validationMessages[]` entry with state `Error`. Exposes `ValidationMessages` (the errors, never empty) and `Warnings` as typed `ValidationMessage` lists. The exception message appends the affected property paths and message texts as returned by DHL (`property: message`, joined with ` | `); Parcel.NET adds no request values, but cannot guarantee that DHL's texts never quote input values. Responses with warnings only, or without validation messages, still throw `ShippingException`.

### Changed

- **Behavior change:** error messages of all DHL clients (Shipping, Tracking, Unified Tracking, Internetmarke, Location Finder, Pickup, Returns) no longer contain the raw response body, which may include addresses and other personal data. If the body carries no `status.detail`/`detail`, the message now reads `DHL <API> returned <code> without error details.` The raw body remains available via `ParcelException.RawResponse`; callers that parsed `Exception.Message` should use `RawResponse` or `DhlShippingValidationException` instead.
- Package descriptions and READMEs state that the project is unofficial and not affiliated with Deutsche Post AG / DHL Group.
- Updated dependencies.

### Fixed

- `DhlShippingClient.ValidateShipmentAsync` no longer fails on unexpected `validationState` values (e.g. non-string types); `Error` is now matched case-insensitively and any other or missing state is reported as `Warning`, consistent with `DhlShippingValidationException`.
- `DhlShippingClient.ValidateShipmentAsync` throws `ShippingException` (with status code and raw response) instead of `JsonException` when an error response is not valid JSON.
- Parsing DHL error bodies never throws, including for bodies with undecodable strings (e.g. unpaired surrogate escapes).

## [LetterXpress 1.0.0] - 2026-06-23

Packages: Parcel.NET.LetterXpress, Parcel.NET.LetterXpress.Letters, Parcel.NET.LetterXpress.All

### Added

- LetterXpress (A&O Fischer) API v3 carrier: `Parcel.NET.LetterXpress` (`LetterXpressOptions`, `LetterXpressException`, DI builder via `AddLetterXpress`), `Parcel.NET.LetterXpress.Letters` and the `Parcel.NET.LetterXpress.All` meta-package.
- `ILetterXpressClient` (`AddLetterXpressLetters`) covering print jobs, SMART@MAIL e-mail jobs, account balance, price calculation, transactions and invoices, with request validation.

## [Abstractions 1.0.2] - 2026-06-05

Packages: Parcel.NET.Abstractions

### Changed

- No functional changes; released alongside DHL 1.0.2.

## [All 1.0.2] - 2026-06-05

Packages: Parcel.NET.All

### Changed

- Updated dependencies: references the 1.0.2 releases of the DHL and GoExpress families.

## [DHL 1.0.2] - 2026-06-05

Packages: Parcel.NET.Dhl, Parcel.NET.Dhl.Shipping, Parcel.NET.Dhl.Tracking, Parcel.NET.Dhl.UnifiedTracking, Parcel.NET.Dhl.Pickup, Parcel.NET.Dhl.Returns, Parcel.NET.Dhl.Internetmarke, Parcel.NET.Dhl.LocationFinder, Parcel.NET.Dhl.All

### Added

- `DhlRetoureService.ReturnAddress` and `DhlRetoureService.ReturnAddressContact` (`Parcel.NET.Dhl.Shipping`) to set the return address of a DHL Retoure service.

### Changed

- Updated dependencies.

### Fixed

- `DhlShippingClient` now sends the `returnAddress` of the DHL Retoure service, which the DHL Parcel DE Shipping API v2 requires; shipments with a return label previously failed validation with "ReturnAddress missing". The address is sent as a DHL `ContactAddress` (including state, phone and contact name) and defaults to the shipper address if no return address is set.

## [GoExpress 1.0.2] - 2026-06-05

Packages: Parcel.NET.GoExpress, Parcel.NET.GoExpress.Shipping, Parcel.NET.GoExpress.Tracking, Parcel.NET.GoExpress.All

### Changed

- Updated dependencies.

## [Abstractions 1.0.1] - 2026-02-21

Packages: Parcel.NET.Abstractions

### Added

- Package-specific README shown on nuget.org.

## [All 1.0.1] - 2026-02-21

Packages: Parcel.NET.All

### Added

- Package-specific README shown on nuget.org.

### Changed

- References the 1.0.1 releases of the DHL and GoExpress families.

## [All 1.0.0] - 2026-02-21

Packages: Parcel.NET.All

### Added

- `Parcel.NET.All` meta-package that includes `Parcel.NET.Dhl.All` and `Parcel.NET.GoExpress.All`.

## [DHL 1.0.1] - 2026-02-21

Packages: Parcel.NET.Dhl, Parcel.NET.Dhl.Shipping, Parcel.NET.Dhl.Tracking, Parcel.NET.Dhl.UnifiedTracking, Parcel.NET.Dhl.Pickup, Parcel.NET.Dhl.Returns, Parcel.NET.Dhl.Internetmarke, Parcel.NET.Dhl.LocationFinder, Parcel.NET.Dhl.All

### Added

- Package-specific README shown on nuget.org.

## [DHL LocationFinder 1.0.0] - 2026-02-21

Packages: Parcel.NET.Dhl.LocationFinder

### Added

- `Parcel.NET.Dhl.LocationFinder`: DHL Location Finder Unified API v1 client (`IDhlLocationFinderClient`: `FindByAddressAsync`, `FindByGeoAsync`, `FindByKeywordAsync`, `GetLocationByIdAsync`), registered via `AddDhlLocationFinder`. The source is identical to the one referenced by `Parcel.NET.Dhl.All` 1.0.0; the package itself was first tagged one day after the rest of the DHL family.

## [GoExpress 1.0.1] - 2026-02-21

Packages: Parcel.NET.GoExpress, Parcel.NET.GoExpress.Shipping, Parcel.NET.GoExpress.Tracking, Parcel.NET.GoExpress.All

### Added

- Package-specific README shown on nuget.org.

## [Abstractions 1.0.0] - 2026-02-20

Packages: Parcel.NET.Abstractions

### Added

- Initial project structure with carrier-agnostic abstractions
- `IShipmentService` and `ITrackingService` interfaces
- Custom exception hierarchy (`ParcelException`, `ShippingException`, `TrackingException`)
- Central package management (`Directory.Packages.props`)
- SourceLink and symbol package support
- DocFX configuration for API documentation generation
- Console sample project

## [DHL 1.0.0] - 2026-02-20

Packages: Parcel.NET.Dhl, Parcel.NET.Dhl.Shipping, Parcel.NET.Dhl.Tracking, Parcel.NET.Dhl.UnifiedTracking, Parcel.NET.Dhl.Pickup, Parcel.NET.Dhl.Returns, Parcel.NET.Dhl.Internetmarke, Parcel.NET.Dhl.All

### Added

- DHL Parcel DE Shipping API v2 client (`Parcel.NET.Dhl.Shipping`)
- DHL Shipment Tracking API v1 client (`Parcel.NET.Dhl.Tracking`)
- `Parcel.NET.Dhl.Tracking` is built on the DHL Parcel DE Tracking API v0 (XML); `Parcel.NET.Dhl.UnifiedTracking` is a separate JSON client for the DHL Unified Shipment Tracking API.
- OAuth token management with caching (`DhlTokenService`)
- Dependency injection extensions with builder pattern
- JSON source generators for AOT compatibility
- DHL Parcel DE Pickup API v3 client (`Parcel.NET.Dhl.Pickup`, `IDhlPickupClient`).
- DHL Parcel DE Returns API v1 client (`Parcel.NET.Dhl.Returns`, `IDhlReturnsClient`).
- DHL Post DE Internetmarke API v1 client (`Parcel.NET.Dhl.Internetmarke`, `IDhlInternetmarkeClient`).
- `Parcel.NET.Dhl.All` meta-package.

## [GoExpress 1.0.0] - 2026-02-20

Packages: Parcel.NET.GoExpress, Parcel.NET.GoExpress.Shipping, Parcel.NET.GoExpress.Tracking, Parcel.NET.GoExpress.All

### Added

- GO! Express & Logistics API clients: `Parcel.NET.GoExpress` (authentication, `GoExpressOptions` including a separate `TrackingBaseUrl`, DI builder via `AddGoExpress`), `Parcel.NET.GoExpress.Shipping` (`IGoExpressShippingClient`: realtime order and label, `GenerateLabelAsync`), `Parcel.NET.GoExpress.Tracking` (`GoExpressTrackingClient`, `ITrackingService`) and the `Parcel.NET.GoExpress.All` meta-package.

[Unreleased]: https://github.com/emuuu/Parcel.NET/commits/main
[All 1.1.0]: https://github.com/emuuu/Parcel.NET/compare/all/v1.0.2...all/v1.1.0
[GoExpress 1.1.0]: https://github.com/emuuu/Parcel.NET/compare/goexpress/v1.0.2...goexpress/v1.1.0
[LetterXpress 1.1.0]: https://github.com/emuuu/Parcel.NET/compare/letterxpress/v1.0.1...letterxpress/v1.1.0
[LetterXpress 1.0.1]: https://github.com/emuuu/Parcel.NET/compare/letterxpress/v1.0.0...letterxpress/v1.0.1
[Abstractions 1.1.0]: https://github.com/emuuu/Parcel.NET/compare/abstractions/v1.0.2...abstractions/v1.1.0
[DHL 1.1.0]: https://github.com/emuuu/Parcel.NET/compare/dhl/v1.0.2...dhl/v1.1.0
[LetterXpress 1.0.0]: https://github.com/emuuu/Parcel.NET/releases/tag/letterxpress/v1.0.0
[Abstractions 1.0.2]: https://github.com/emuuu/Parcel.NET/compare/abstractions/v1.0.1...abstractions/v1.0.2
[All 1.0.2]: https://github.com/emuuu/Parcel.NET/compare/all/v1.0.1...all/v1.0.2
[DHL 1.0.2]: https://github.com/emuuu/Parcel.NET/compare/dhl/v1.0.1...dhl/v1.0.2
[GoExpress 1.0.2]: https://github.com/emuuu/Parcel.NET/compare/goexpress/v1.0.1...goexpress/v1.0.2
[Abstractions 1.0.1]: https://github.com/emuuu/Parcel.NET/compare/abstractions/v1.0.0...abstractions/v1.0.1
[All 1.0.1]: https://github.com/emuuu/Parcel.NET/compare/all/v1.0.0...all/v1.0.1
[All 1.0.0]: https://github.com/emuuu/Parcel.NET/releases/tag/all/v1.0.0
[DHL 1.0.1]: https://github.com/emuuu/Parcel.NET/compare/dhl/v1.0.0...dhl/v1.0.1
[DHL LocationFinder 1.0.0]: https://github.com/emuuu/Parcel.NET/releases/tag/dhl-locationfinder/v1.0.0
[GoExpress 1.0.1]: https://github.com/emuuu/Parcel.NET/compare/goexpress/v1.0.0...goexpress/v1.0.1
[Abstractions 1.0.0]: https://github.com/emuuu/Parcel.NET/releases/tag/abstractions/v1.0.0
[DHL 1.0.0]: https://github.com/emuuu/Parcel.NET/releases/tag/dhl/v1.0.0
[GoExpress 1.0.0]: https://github.com/emuuu/Parcel.NET/releases/tag/goexpress/v1.0.0
