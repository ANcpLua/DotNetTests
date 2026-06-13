namespace AdvancedPatterns.ChatPipeline;

public sealed class ChatPipelineExpectationTests
{
    [Fact]
    public async Task RunAsync_SendsPersistedHistoryAndCapturesEveryServiceCall()
    {
        ChatPipelineHarness harness = new(
            new ServiceCallExpectation(
                ResponseText: "Paris.",
                VerifyInput: messages => Assert.Equal(["What is the capital of France?"], messages.Select(m => m.Text))),
            new ServiceCallExpectation(
                ResponseText: "Vienna.",
                VerifyInput: messages => Assert.Equal(
                    ["What is the capital of France?", "Paris.", "And Austria?"],
                    messages.Select(m => m.Text))));

        ChatPipeline pipeline = harness.CreatePipeline();

        ChatMessage first = await pipeline.RunAsync("What is the capital of France?");
        ChatMessage second = await pipeline.RunAsync("And Austria?");

        Assert.Equal("Paris.", first.Text);
        Assert.Equal("Vienna.", second.Text);
        Assert.Equal(2, harness.TotalServiceCalls);
        Assert.Equal(
            ["What is the capital of France?", "Paris.", "And Austria?", "Vienna."],
            pipeline.History.Select(m => m.Text));
    }

    [Fact]
    public async Task RunAsync_FailsWhenServiceReceivesUnexpectedExtraCall()
    {
        ChatPipelineHarness harness = new(new ServiceCallExpectation("done"));
        ChatPipeline pipeline = harness.CreatePipeline();

        _ = await pipeline.RunAsync("first");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => pipeline.RunAsync("second"));

        Assert.Contains("unexpected service call #2", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed class ChatPipelineHarness(params ServiceCallExpectation[] expectations)
{
    private readonly ScriptedChatService _service = new(expectations);

    public int TotalServiceCalls => this._service.TotalCalls;

    public ChatPipeline CreatePipeline() => new(this._service);
}

internal sealed class ChatPipeline(IChatService service)
{
    private readonly List<ChatMessage> _history = [];

    public IReadOnlyList<ChatMessage> History => this._history;

    public async Task<ChatMessage> RunAsync(string userText)
    {
        this._history.Add(new ChatMessage(ChatRole.User, userText));

        ChatMessage response = await service.GetResponseAsync(this._history);
        this._history.Add(response);

        return response;
    }
}

internal interface IChatService
{
    Task<ChatMessage> GetResponseAsync(IReadOnlyList<ChatMessage> messages);
}

internal sealed class ScriptedChatService(IReadOnlyList<ServiceCallExpectation> expectations) : IChatService
{
    private int _callIndex;

    public int TotalCalls => this._callIndex;

    public Task<ChatMessage> GetResponseAsync(IReadOnlyList<ChatMessage> messages)
    {
        int callNumber = this._callIndex + 1;
        if (this._callIndex >= expectations.Count)
        {
            throw new InvalidOperationException(
                $"Received unexpected service call #{callNumber}; only {expectations.Count} call(s) were expected.");
        }

        ServiceCallExpectation expectation = expectations[this._callIndex++];
        expectation.VerifyInput?.Invoke(messages);

        return Task.FromResult(new ChatMessage(ChatRole.Assistant, expectation.ResponseText));
    }
}

internal sealed record ServiceCallExpectation(
    string ResponseText,
    Action<IReadOnlyList<ChatMessage>>? VerifyInput = null);

internal sealed record ChatMessage(ChatRole Role, string Text);

internal enum ChatRole
{
    User,
    Assistant
}
