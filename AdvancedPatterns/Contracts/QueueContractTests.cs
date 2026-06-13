namespace AdvancedPatterns.Contracts;

public interface IAsyncQueue
{
    ValueTask EnqueueAsync(string item);

    ValueTask<string?> DequeueAsync();

    ValueTask<int> CountAsync();
}

public interface IQueueFixture
{
    IAsyncQueue CreateQueue();
}

public abstract class QueueContractTests<TFixture>
    where TFixture : IQueueFixture, new()
{
    private readonly TFixture _fixture = new();

    [Fact]
    public async Task DequeueAsync_ReturnsItemsInInsertionOrder()
    {
        IAsyncQueue queue = this._fixture.CreateQueue();

        await queue.EnqueueAsync("first");
        await queue.EnqueueAsync("second");

        Assert.Equal("first", await queue.DequeueAsync());
        Assert.Equal("second", await queue.DequeueAsync());
        Assert.Null(await queue.DequeueAsync());
    }

    [Fact]
    public async Task CountAsync_TracksEnqueuedAndDequeuedItems()
    {
        IAsyncQueue queue = this._fixture.CreateQueue();

        Assert.Equal(0, await queue.CountAsync());

        await queue.EnqueueAsync("alpha");
        await queue.EnqueueAsync("beta");
        Assert.Equal(2, await queue.CountAsync());

        _ = await queue.DequeueAsync();
        Assert.Equal(1, await queue.CountAsync());
    }
}

public sealed class InMemoryQueueContractTests : QueueContractTests<InMemoryQueueFixture>;

public sealed class InMemoryQueueFixture : IQueueFixture
{
    public IAsyncQueue CreateQueue() => new InMemoryQueue();
}

internal sealed class InMemoryQueue : IAsyncQueue
{
    private readonly Queue<string> _items = new();

    public ValueTask EnqueueAsync(string item)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(item);

        this._items.Enqueue(item);
        return ValueTask.CompletedTask;
    }

    public ValueTask<string?> DequeueAsync() =>
        ValueTask.FromResult(this._items.TryDequeue(out string? item) ? item : null);

    public ValueTask<int> CountAsync() => ValueTask.FromResult(this._items.Count);
}
