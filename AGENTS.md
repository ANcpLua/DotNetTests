# DotNetTests — Agent Notes

Living reference comparing six .NET test frameworks (TUnit, xUnit, xUnit v3, NUnit, MSTest, bUnit) against the same surface area: a Calculator unit, an Aspire-orchestrated WebApp + ApiService, a Blazor Counter component.

## Repo layout

Tests live at the repo root because they are the point. The system-under-test is hidden under `src/`.

```
src/                  System under test (hidden — the focus is on tests)
  AppHost/            .NET Aspire orchestrator (Redis + ApiService + WebApp)
  ApiService/         Minimal API exposing /weatherforecast
  WebApp/             Blazor Server frontend (Counter.razor, Weather.razor)
  ServiceDefaults/    Shared OpenTelemetry + health checks + service discovery

UnitTests/            TUnit — Calculator unit tests, hooks, data-driven, DI patterns
IntegrationTests/     TUnit.Aspire — distributed app fixture, ApiTests
PlaywrightTests/      TUnit.Playwright — browser-driven E2E
XUnit/                xUnit v2 — parallel CalculatorTests in xUnit idiom
XUnitV3/              xUnit v3 (Microsoft Testing Platform)
NUnit/                NUnit 4 — constraint-based assertions
MSTest/               MSTest 4 — Microsoft Testing Platform
BUnit/                bUnit — renders Counter.razor without browser
AdvancedPatterns/     xUnit v3 — reusable high-leverage test architecture patterns
```

## Solution files

Eight `.slnx` files at root, scope from narrow to wide:

| File | Scope |
| --- | --- |
| `DotNetTests.slnx` | Master — all 13 projects, grouped by framework |
| `TUnit.Playground.slnx` | TUnit stack + Aspire app — primary "live" entry point |
| `AdvancedPatterns.slnx` | Advanced xUnit v3 test architecture pattern examples |
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
dotnet run --project AdvancedPatterns/AdvancedPatterns.Tests.csproj --no-build

# Run the live app (Aspire dashboard at https://localhost:17xxx)
cd src/AppHost && dotnet run
```

## Framework versions

Renovate bumps pinned versions continuously — the csproj files are the source of truth.

| Package | Notes |
| --- | --- |
| TUnit, TUnit.Aspire, TUnit.Playwright | Floating range — weekly releases |
| xunit | v2.x stable line |
| xunit.v3.mtp-v2 | v3 — runs on Microsoft Testing Platform |
| bunit | Blazor component renderer |
| Aspire.Hosting.* | .NET 10 target |

## CI and automation

- `.github/workflows/ci.yml` builds the solution then matrix-tests the six framework projects. IntegrationTests, PlaywrightTests, and AdvancedPatterns are not in the test matrix (Docker / Playwright browsers — run them locally).
- `.github/workflows/auto-merge.yml` (fleet-synced — do not hand-edit) enables native GitHub auto-merge for `codex/` and `copilot/` branches and Codex-connector-approved PRs. Branch protection on main requires CI to pass before the merge fires.
- `renovate.json` extends the shared `github>ANcpLua/github-settings-automation` preset for dependency bumps.

## Conventions in this repo

- **XUnit, XUnitV3, NUnit, and MSTest each duplicate `Calculator` as a `file class`** (TUnit's lives in `UnitTests/Calculator.cs` as a public class) — intentional. Keeps each test project self-contained so syntax differences stand out. Don't extract to a shared class library.
- **Project name ≠ NuGet package name** — csproj files use `.Tests` suffix (`XUnit.Tests.csproj` in folder `XUnit/`). Required: NuGet's NU1108 cycle detection treats matching names as self-references.
- **Aspire `Projects.X` types** are generated from csproj filename (not folder path). Renaming a csproj breaks `src/AppHost/Program.cs` and `IntegrationTests/AppFixture.cs` — update both.
- **Namespaces match folder, not csproj name** — `namespace XUnit.Tests` lives in `XUnit/XUnit.Tests.csproj`.
- **PlaywrightTests** auto-installs browsers on first run via `Microsoft.Playwright.Program.Main(["install"])` in `Hooks.cs`.
- **IntegrationTests** require Docker for Redis container (Aspire's `AddRedis("cache")`).
