---
title: AOT Compatibility
category: Guides
order: 3
description: Native AOT and trimming support via JSON source generators.
---

## Overview

ParcelNET is designed for Native AOT (Ahead-of-Time) compilation and assembly trimming. All JSON serialization uses compile-time source generators instead of runtime reflection.

## JSON Source Generators

Each carrier module includes a `JsonSerializerContext` for its API models:

| Module | Context Class | Purpose |
|--------|--------------|---------|
| `ParcelNET.Dhl` | `DhlAuthJsonContext` | OAuth token response serialization |
| `ParcelNET.Dhl.Shipping` | `DhlShippingJsonContext` | Shipping request/response models |
| `ParcelNET.Dhl.Tracking` | `DhlTrackingJsonContext` | Tracking response models |

## How It Works

Instead of using reflection-based `JsonSerializer`, ParcelNET uses source-generated serializers:

```csharp
// Source generator declaration (internal)
[JsonSerializable(typeof(DhlTrackingResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class DhlTrackingJsonContext : JsonSerializerContext;
```

This approach:
- Generates serialization code at compile time
- Eliminates runtime reflection
- Enables full trimming without warnings
- Reduces startup time

## Trimming Support

ParcelNET libraries are fully trimmable. No trimming warnings are produced when publishing with:

```bash
dotnet publish -c Release -p:PublishTrimmed=true
```

## Native AOT Publishing

Publish your application as a native AOT binary:

```bash
dotnet publish -c Release -p:PublishAot=true
```

All ParcelNET packages are compatible with:
- `PublishTrimmed=true`
- `PublishAot=true`
- `TrimMode=full`

## What This Means for You

As a consumer of ParcelNET, you don't need to do anything special. The source generators are internal to the library and handle all JSON operations transparently. Your application can use Native AOT or trimming without any additional configuration related to ParcelNET.
