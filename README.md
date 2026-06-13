# DotNetTests

Six .NET test frameworks side-by-side, tested against the same code: a `Calculator`, an Aspire-orchestrated WebApp + ApiService, a Blazor `Counter` component.

Use this repo to read the same test written six different ways.

`AdvancedPatterns/` is the extra study project: reusable contract tests, in-memory protocol hosts, deterministic call expectations, JSON workflow harnesses, and adversarial file-store tests.

## Frameworks

| Folder | Package | Version |
| --- | --- | --- |
| `UnitTests/` `IntegrationTests/` `PlaywrightTests/` | [TUnit](https://tunit.dev) | `1.*` |
| `XUnit/` | [xUnit v2](https://xunit.net) | `2.9.3` |
| `XUnitV3/` | [xUnit v3](https://xunit.net) | `3.2.2` |
| `NUnit/` | [NUnit 4](https://docs.nunit.org) | `4.6.0` |
| `MSTest/` | [MSTest 4](https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-intro) | `4.2.3` |
| `BUnit/` | [bUnit](https://bunit.dev) | `2.7.2` |
| `AdvancedPatterns/` | xUnit v3 + ASP.NET `TestServer` | `3.2.2` |

## Run

```bash
dotnet build DotNetTests.slnx

dotnet test XUnit/XUnit.Tests.csproj
dotnet test NUnit/NUnit.Tests.csproj
dotnet test MSTest/MSTest.Tests.csproj
dotnet test BUnit/BUnit.Tests.csproj

cd UnitTests && dotnet run    # TUnit
cd XUnitV3   && dotnet run    # xUnit v3
dotnet run --project AdvancedPatterns/AdvancedPatterns.Tests.csproj
```

## Live app

```bash
cd src/AppHost && dotnet run
```

Open the Aspire dashboard URL printed in the console. Requires Docker (for Redis).

## Requirements

.NET 10 SDK · Docker (for `AppHost` and `IntegrationTests`)

## Read first

`AGENTS.md` — layout, conventions, framework-specific gotchas.
