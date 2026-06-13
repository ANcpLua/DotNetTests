# AdvancedPatterns

High-leverage .NET test architecture patterns distilled from the Agent Framework test-suite review.

This project intentionally favors small, deterministic examples over copied production source:

- `Contracts/` — reusable behavior contracts implemented once and inherited by concrete fixture tests.
- `Protocol/` — a real `WebApplicationBuilder` + `TestServer` host with deterministic in-memory services.
- `ChatPipeline/` — ordered service-call expectations that capture request history and fail on unexpected calls.
- `Workflows/` — JSON-driven workflow tests with event-sequence validation and checkpoint/resume flow.
- `Security/` — adversarial file-store tests for path traversal and outside-root access.

Run it with:

```bash
dotnet run --project AdvancedPatterns/AdvancedPatterns.Tests.csproj
```
