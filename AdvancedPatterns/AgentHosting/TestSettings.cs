namespace AdvancedPatterns.Tests.AgentHosting;

/// <summary>
/// Configuration keys for provider-backed live tests, resolved from environment variables.
/// </summary>
/// <remarks>
/// Extracted from <c>dotnet/src/Shared/IntegrationTests/TestSettings.cs</c> in
/// microsoft/agent-framework, where it is MSBuild-injected into every integration-test project via
/// the <c>InjectSharedIntegrationTestCode</c> property. Trimmed to the keys these tests read; the
/// upstream file also carries Azure AI, Copilot Studio, Mem0, and OpenAI keys.
/// </remarks>
internal static class TestSettings
{
    // Anthropic
    public const string AnthropicApiKey = "ANTHROPIC_API_KEY";
    public const string AnthropicChatModelName = "ANTHROPIC_CHAT_MODEL_NAME";
    public const string AnthropicReasoningModelName = "ANTHROPIC_REASONING_MODEL_NAME";
    public const string AnthropicServiceId = "ANTHROPIC_SERVICE_ID";
}
