using System.Text.Json;
using Anthropic;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.OpenAI;

namespace AdvancedPatterns.Tests.AgentHosting;

/// <summary>
/// Live tests for the app-owned routing helper surface (<see cref="OpenAIResponses"/> plus
/// <see cref="AgentSessionStore"/>) exercised against a real Anthropic model. These confirm the crucial
/// consumption paths — request conversion, an agent run, response rendering, and multi-turn session
/// continuity — behave correctly end to end when the hosted agent is backed by an Anthropic chat client.
/// </summary>
/// <remarks>
/// <para>
/// The pattern under demonstration: the Responses-shaped hosting surface is provider-agnostic. It
/// converts an HTTP body into an agent run and renders the result back, so the same helpers serve an
/// Anthropic-backed agent as readily as an OpenAI-backed one. Only agent construction differs.
/// </para>
/// <para>
/// Skipped unless <c>ANTHROPIC_API_KEY</c> is set, so runs without secrets stay green. Adapted from
/// <c>OpenAIResponsesHostingLiveTests</c> in microsoft/agent-framework
/// (<c>dotnet/tests/Microsoft.Agents.AI.Hosting.OpenAI.IntegrationTests</c>).
/// </para>
/// </remarks>
public sealed class AnthropicResponsesHostingLiveTests
{
    private static string? ApiKey => Environment.GetEnvironmentVariable(TestSettings.AnthropicApiKey);

    private static string ModelName =>
        Environment.GetEnvironmentVariable(TestSettings.AnthropicChatModelName) ?? "claude-opus-5";

    [Fact]
    public async Task NonStreamingRun_RendersResponsesShapedPayloadAsync()
    {
        // Arrange
        Assert.SkipWhen(string.IsNullOrEmpty(ApiKey), "ANTHROPIC_API_KEY is not configured; skipping live hosting test.");
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        AIAgent agent = CreateAgent();
        AgentSessionStore sessionStore = new InMemoryAgentSessionStore();
        JsonElement body = ParseBody("""{ "input": "Reply with exactly the word: apple" }""");

        // Act
        OpenAIResponsesRunRequest run = OpenAIResponses.ToAgentRunRequest(body);
        string sessionStoreId = OpenAIResponses.GetSessionStoreId(run) ?? OpenAIResponses.CreateResponseId();
        AgentSession session = await sessionStore.GetSessionAsync(agent, sessionStoreId, cancellationToken);
        string responseId = OpenAIResponses.CreateResponseId();
        AgentResponse result = await agent.RunAsync(run.Messages, session, run.Options, cancellationToken);
        JsonElement payload = OpenAIResponses.WriteResponse(result, responseId, responseId);

        // Assert
        Assert.Equal(responseId, payload.GetProperty("id").GetString());
        Assert.Equal("response", payload.GetProperty("object").GetString());
        Assert.Contains("output", payload.EnumerateObject().Select(p => p.Name));
    }

    [Fact]
    public async Task MultiTurn_ContinuesSessionAcrossTurnsAsync()
    {
        // Arrange
        Assert.SkipWhen(string.IsNullOrEmpty(ApiKey), "ANTHROPIC_API_KEY is not configured; skipping live hosting test.");
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        AIAgent agent = CreateAgent();
        AgentSessionStore sessionStore = new InMemoryAgentSessionStore();

        // Act: first turn establishes context, second turn continues from the first response id.
        string firstResponseId = await RunTurnAsync(agent, sessionStore, """{ "input": "Remember the number 7." }""", cancellationToken);
        JsonElement secondBody = ParseBody($$"""{ "input": "What number did I ask you to remember?", "previous_response_id": "{{firstResponseId}}" }""");
        OpenAIResponsesRunRequest secondRun = OpenAIResponses.ToAgentRunRequest(secondBody);
        string secondSessionStoreId = OpenAIResponses.GetSessionStoreId(secondRun)!;
        AgentSession session = await sessionStore.GetSessionAsync(agent, secondSessionStoreId, cancellationToken);
        AgentResponse secondResult = await agent.RunAsync(secondRun.Messages, session, secondRun.Options, cancellationToken);

        // Assert: continuation succeeded and the model produced a textual answer.
        Assert.Equal(secondSessionStoreId, firstResponseId);
        Assert.False(string.IsNullOrWhiteSpace(secondResult.Text));
    }

    private static async Task<string> RunTurnAsync(
        AIAgent agent,
        AgentSessionStore sessionStore,
        string bodyJson,
        CancellationToken cancellationToken)
    {
        JsonElement body = ParseBody(bodyJson);
        OpenAIResponsesRunRequest run = OpenAIResponses.ToAgentRunRequest(body);
        string sessionStoreId = OpenAIResponses.GetSessionStoreId(run) ?? OpenAIResponses.CreateResponseId();
        AgentSession session = await sessionStore.GetSessionAsync(agent, sessionStoreId, cancellationToken);
        string responseId = OpenAIResponses.CreateResponseId();
        _ = await agent.RunAsync(run.Messages, session, run.Options, cancellationToken);
        await sessionStore.SaveSessionAsync(agent, responseId, session, cancellationToken);
        return responseId;
    }

    private static ChatClientAgent CreateAgent() =>
        new AnthropicClient { ApiKey = ApiKey }.AsAIAgent(
            ModelName,
            instructions: "You are a concise assistant.",
            name: "assistant");

    private static JsonElement ParseBody(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
