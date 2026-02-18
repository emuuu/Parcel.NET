# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Initial project structure with carrier-agnostic abstractions
- `IShipmentService` and `ITrackingService` interfaces
- DHL Parcel DE Shipping API v2 client (`ParcelNET.Dhl.Shipping`)
- DHL Shipment Tracking API v1 client (`ParcelNET.Dhl.Tracking`)
- OAuth token management with caching (`DhlTokenService`)
- Custom exception hierarchy (`ParcelNetException`, `ShippingException`, `TrackingException`)
- Dependency injection extensions with builder pattern
- JSON source generators for AOT compatibility
- Central package management (`Directory.Packages.props`)
- SourceLink and symbol package support
- DocFX configuration for API documentation generation
- Console sample project

[Unreleased]: https://github.com/ParcelNET/ParcelNET/commits/main
