using System.Text.Json;

namespace AdvancedPatterns.Workflows;

public sealed class WorkflowHarnessTests
{
    [Fact]
    public async Task RunAsync_ReplaysJsonTestCaseThroughCheckpointedWorkflow()
    {
        WorkflowTestCase testcase = await WorkflowTestCase.LoadAsync("approval-workflow.json");
        WorkflowHarness harness = new(new DeterministicWorkflow());

        WorkflowRunResult result = await harness.RunAsync(testcase);

        Assert.Equal(testcase.ExpectedEvents, result.Events.Select(e => e.Name));
        Assert.Equal(testcase.ExpectedOutput, result.Output);
    }
}

internal sealed class WorkflowHarness(IWorkflow workflow)
{
    public async Task<WorkflowRunResult> RunAsync(WorkflowTestCase testcase)
    {
        WorkflowRun run = await workflow.StartAsync(testcase.Input);
        List<WorkflowEvent> events = [.. run.Events];

        while (run.PendingRequest is not null)
        {
            string response = testcase.Responses.DequeueOrThrow(
                $"No scripted response exists for request '{run.PendingRequest}'.");

            run = await workflow.ResumeAsync(run.Checkpoint, response);
            events.AddRange(run.Events);
        }

        return new WorkflowRunResult(events, run.Output);
    }
}

internal interface IWorkflow
{
    Task<WorkflowRun> StartAsync(string input);

    Task<WorkflowRun> ResumeAsync(WorkflowCheckpoint checkpoint, string response);
}

internal sealed class DeterministicWorkflow : IWorkflow
{
    public Task<WorkflowRun> StartAsync(string input) =>
        Task.FromResult(new WorkflowRun(
            Events:
            [
                new("Started"),
                new("Agent:planner"),
                new("Request:approval")
            ],
            Checkpoint: new WorkflowCheckpoint(input),
            PendingRequest: "approval",
            Output: null));

    public Task<WorkflowRun> ResumeAsync(WorkflowCheckpoint checkpoint, string response) =>
        Task.FromResult(new WorkflowRun(
            Events:
            [
                new($"Response:{response}"),
                new("Agent:executor"),
                new("Completed")
            ],
            Checkpoint: checkpoint,
            PendingRequest: null,
            Output: $"{checkpoint.Input}:{response}"));
}

internal sealed record WorkflowRun(
    IReadOnlyList<WorkflowEvent> Events,
    WorkflowCheckpoint Checkpoint,
    string? PendingRequest,
    string? Output);

internal sealed record WorkflowRunResult(IReadOnlyList<WorkflowEvent> Events, string? Output);

internal sealed record WorkflowEvent(string Name);

internal sealed record WorkflowCheckpoint(string Input);

internal sealed class WorkflowTestCase
{
    public required string Description { get; init; }

    public required string Input { get; init; }

    public required Queue<string> Responses { get; init; }

    public required string[] ExpectedEvents { get; init; }

    public required string ExpectedOutput { get; init; }

    public static async Task<WorkflowTestCase> LoadAsync(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Workflows", "TestCases", fileName);
        await using FileStream stream = File.OpenRead(path);

        WorkflowTestCase? testcase = await JsonSerializer.DeserializeAsync<WorkflowTestCase>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return testcase ?? throw new InvalidOperationException($"Could not deserialize workflow testcase '{path}'.");
    }
}

internal static class QueueExtensions
{
    public static T DequeueOrThrow<T>(this Queue<T> queue, string message) =>
        queue.TryDequeue(out T? item) ? item : throw new InvalidOperationException(message);
}
