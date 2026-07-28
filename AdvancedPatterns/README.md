# AdvancedPatterns

High-leverage .NET test architecture patterns distilled from the Agent Framework test-suite review.

This project intentionally favors small, deterministic examples over copied production source:

- `Contracts/` — reusable behavior contracts implemented once and inherited by concrete fixture tests.
- `Protocol/` — a real `WebApplicationBuilder` + `TestServer` host with deterministic in-memory services.
- `ChatPipeline/` — ordered service-call expectations that capture request history and fail on unexpected calls.
- `Workflows/` — JSON-driven workflow tests with event-sequence validation and checkpoint/resume flow.
- `Security/` — adversarial file-store tests for path traversal and outside-root access.
- `AgentHosting/` — the one live-service exception: proves the Agent Framework's Responses-shaped
  hosting surface is provider-agnostic by driving it with a real Anthropic model. Skips itself unless
  `ANTHROPIC_API_KEY` is set, so a secret-less run stays green.

Run it with:

```bash
dotnet run --project AdvancedPatterns/AdvancedPatterns.Tests.csproj
```

## AgentHosting — running the live tests

`AgentHosting/` is extracted from microsoft/agent-framework's
`Microsoft.Agents.AI.Hosting.OpenAI.IntegrationTests`, with the agent rebuilt on
`AnthropicClient.AsAIAgent(...)`. Upstream injects `TestSettings` into every integration-test project
via MSBuild (`InjectSharedIntegrationTestCode`); here it is vendored as `AgentHosting/TestSettings.cs`,
trimmed to the keys these tests read.

The helpers under test (`OpenAIResponses`, `AgentSessionStore`) carry the OpenAI name because they
implement OpenAI's Responses *wire format* — they are not bound to OpenAI as a model provider, which is
exactly what these tests demonstrate.

```bash
# Skips without a key — this is the default, and it is what CI would see.
dotnet run --project AdvancedPatterns/AdvancedPatterns.Tests.csproj

# Live: bills real tokens against your Anthropic account.
ANTHROPIC_API_KEY=sk-ant-... \
ANTHROPIC_CHAT_MODEL_NAME=claude-haiku-4-5 \
  dotnet run --project AdvancedPatterns/AdvancedPatterns.Tests.csproj
```

`ANTHROPIC_CHAT_MODEL_NAME` is optional and defaults to `claude-opus-5`; set it to `claude-haiku-4-5`
for the cheap path, since neither test needs a frontier model to pass.
