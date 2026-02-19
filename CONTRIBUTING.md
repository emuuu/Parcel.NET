# Contributing to Parcel.NET

Thank you for your interest in contributing to Parcel.NET!

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later

## Building

```bash
dotnet build
```

## Running Tests

```bash
dotnet test
```

## Creating NuGet Packages

```bash
dotnet pack
```

## Pull Request Process

1. Fork the repository and create a feature branch from `main`.
2. Ensure `dotnet build` completes without errors or warnings.
3. Ensure `dotnet test` passes with all tests green.
4. Add or update XML documentation on any new or changed public APIs.
5. Update `CHANGELOG.md` under the `[Unreleased]` section.
6. Open a pull request with a clear description of the changes.

## Code Guidelines

- Target **.NET 10.0** (`net10.0`).
- Use **file-scoped namespaces**.
- Use `init` properties on public models.
- Add `ArgumentNullException.ThrowIfNull()` guards on all public method parameters.
- Use the custom exception types (`ShippingException`, `TrackingException`) instead of generic exceptions.
- Add `/// <summary>` XML docs to all public types and members.
- Keep carrier-specific code isolated in the corresponding `Parcel.NET.<Carrier>.*` projects.
- Write tests for all new functionality using xUnit v3, Shouldly, and NSubstitute.

## Adding a New Carrier

1. Create `Parcel.NET.<Carrier>` for shared auth/config.
2. Create `Parcel.NET.<Carrier>.Shipping` and/or `Parcel.NET.<Carrier>.Tracking`.
3. Implement `IShipmentService` and/or `ITrackingService`.
4. Add DI extension methods following the builder pattern (`AddDhl` / `AddDhlShipping`).
5. Add test projects under `tests/`.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
