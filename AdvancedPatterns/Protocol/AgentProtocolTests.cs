using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AdvancedPatterns.Protocol;

public sealed class AgentProtocolTests
{
    [Fact]
    public async Task RunEndpoint_ExecutesRegisteredAgentWithoutLiveServices()
    {
        await using AgentProtocolHost host = await AgentProtocolHost.StartAsync(new ScriptedAgentRuntime(
            expectedAgent: "researcher",
            responseText: "deterministic answer"),
            TestContext.Current.CancellationToken);

        using HttpResponseMessage response = await host.Client.PostAsJsonAsync(
            "/agents/researcher/run",
            new AgentRunRequest("summarize the test strategy"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AgentRunResponse? body = await response.Content.ReadFromJsonAsync<AgentRunResponse>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal("researcher", body.AgentName);
        Assert.Equal("deterministic answer", body.Text);
    }

    [Fact]
    public async Task RunEndpoint_ReturnsNotFoundForUnknownAgent()
    {
        await using AgentProtocolHost host = await AgentProtocolHost.StartAsync(new ScriptedAgentRuntime(
            expectedAgent: "planner",
            responseText: "plan"),
            TestContext.Current.CancellationToken);

        using HttpResponseMessage response = await host.Client.PostAsJsonAsync(
            "/agents/writer/run",
            new AgentRunRequest("draft"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

internal sealed class AgentProtocolHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private AgentProtocolHost(WebApplication app, HttpClient client)
    {
        this._app = app;
        this.Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<AgentProtocolHost> StartAsync(
        IAgentRuntime agentRuntime,
        CancellationToken cancellationToken)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(agentRuntime);

        WebApplication app = builder.Build();

        app.MapPost("/agents/{agentName}/run", async (
            string agentName,
            AgentRunRequest request,
            IAgentRuntime runtime) =>
        {
            AgentRunResponse? response = await runtime.RunAsync(agentName, request.Input);
            return response is null ? Results.NotFound() : Results.Json(response);
        });

        await app.StartAsync(cancellationToken);

        TestServer server = app.Services.GetRequiredService<IServer>() as TestServer
            ?? throw new InvalidOperationException("Expected in-memory TestServer.");

        return new AgentProtocolHost(app, server.CreateClient());
    }

    public async ValueTask DisposeAsync()
    {
        this.Client.Dispose();
        await this._app.DisposeAsync();
    }
}

internal interface IAgentRuntime
{
    Task<AgentRunResponse?> RunAsync(string agentName, string input);
}

internal sealed class ScriptedAgentRuntime(string expectedAgent, string responseText) : IAgentRuntime
{
    public Task<AgentRunResponse?> RunAsync(string agentName, string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        AgentRunResponse? response = string.Equals(agentName, expectedAgent, StringComparison.Ordinal)
            ? new AgentRunResponse(agentName, responseText)
            : null;

        return Task.FromResult(response);
    }
}

internal sealed record AgentRunRequest(string Input);

internal sealed record AgentRunResponse(string AgentName, string Text);
