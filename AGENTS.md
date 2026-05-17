# DotNetTests — Agent Notes

Living reference comparing six .NET test frameworks (TUnit, xUnit, xUnit v3, NUnit, MSTest, bUnit) against the same surface area: a Calculator unit, an Aspire-orchestrated WebApp + ApiService, a Blazor Counter component.

## Repo layout

```
src/
  AppHost/          .NET Aspire orchestrator (Redis + ApiService + WebApp)
  ApiService/       Minimal API exposing /weatherforecast
  WebApp/           Blazor Server frontend (Counter.razor, Weather.razor)
  ServiceDefaults/  Shared OpenTelemetry + health checks + service discovery

tests/
  UnitTests/         TUnit — Calculator unit tests, hooks, data-driven, DI patterns
  IntegrationTests/  TUnit.Aspire — distributed app fixture, ApiTests
  PlaywrightTests/   TUnit.Playwright — browser-driven E2E
  XUnit/             xUnit v2 — parallel CalculatorTests in xUnit idiom
  XUnitV3/           xUnit v3 (Microsoft Testing Platform) — same tests
  NUnit/             NUnit 4 — constraint-based assertions
  MSTest/            MSTest 4 — Microsoft Testing Platform
  BUnit/             bUnit — renders Counter.razor without browser
```

## Solution files

Seven `.slnx` files at root, scope from narrow to wide:

| File | Scope |
| --- | --- |
| `DotNetTests.slnx` | Master — all 12 projects, grouped by framework |
| `TUnit.Playground.slnx` | TUnit stack + Aspire app — primary "live" entry point |
| `BUnit.slnx` | BUnit + WebApp + ServiceDefaults |
| `XUnit.slnx`, `XUnitV3.slnx`, `NUnit.slnx`, `MSTest.slnx` | Isolated single-framework slices |

## Build and test

```bash
# Full build
dotnet build DotNetTests.slnx

# Per-framework tests
dotnet test XUnit/XUnit.Tests.csproj --no-build
dotnet test NUnit/NUnit.Tests.csproj --no-build
dotnet test MSTest/MSTest.Tests.csproj --no-build
dotnet test BUnit/BUnit.Tests.csproj --no-build

# TUnit + xUnit v3 are exe-based (Microsoft Testing Platform)
cd UnitTests && dotnet run --no-build
cd XUnitV3 && dotnet run --no-build

# Run the live app (Aspire dashboard at https://localhost:17xxx)
cd AppHost && dotnet run
```

## Framework versions (verified against nuget.org)

| Package | Version | Notes |
| --- | --- | --- |
| TUnit, TUnit.Aspire, TUnit.Playwright | `1.*` (floats latest) | Weekly releases |
| xunit | `2.9.3` | v2.x stable line |
| xunit.v3.mtp-v2 | `3.2.2` | v3 — runs on Microsoft Testing Platform |
| NUnit | `4.6.0` | |
| MSTest | `4.2.3` | |
| bunit | `2.7.2` | Blazor component renderer |
| Microsoft.NET.Test.Sdk | `18.5.1` | |
| Aspire.Hosting.* | `13.3.3` | .NET 10 target |
| coverlet.collector | `10.0.0` | |

## Conventions in this repo

- **Each framework project duplicates `Calculator` as `file class`** — intentional. Keeps each test project self-contained so syntax differences stand out. Don't extract to shared class library.
- **Project name ≠ NuGet package name** — csproj files use `.Tests` suffix (`XUnit.Tests.csproj` in folder `XUnit/`). Required: NuGet's NU1108 cycle detection treats matching names as self-references.
- **Aspire `Projects.X` types** are generated from csproj filename. Renaming a csproj breaks `AppHost/Program.cs` and `IntegrationTests/AppFixture.cs` — update both.
- **Namespaces match folder, not csproj name** — `namespace XUnit.Tests` lives in `XUnit/XUnit.Tests.csproj`.
- **PlaywrightTests** auto-installs browsers on first run via `Microsoft.Playwright.Program.Main(["install"])` in `Hooks.cs`.
- **IntegrationTests** require Docker for Redis container (Aspire's `AddRedis("cache")`).
