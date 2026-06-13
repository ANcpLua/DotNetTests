namespace AdvancedPatterns.Security;

public sealed class SafeFileStoreTests
{
    [Fact]
    public async Task ReadTextAsync_RejectsParentTraversalOutsideRoot()
    {
        using TempWorkspace workspace = TempWorkspace.Create();
        await File.WriteAllTextAsync(
            workspace.OutsideFile,
            "SECRET_OUTSIDE_ROOT",
            TestContext.Current.CancellationToken);

        SafeFileStore store = new(workspace.Root);

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => store.ReadTextAsync("../outside.txt", TestContext.Current.CancellationToken));

        Assert.Contains("outside the file-store root", exception.Message);
    }

    [Fact]
    public async Task SearchAsync_DoesNotLeakFilesOutsideRoot()
    {
        using TempWorkspace workspace = TempWorkspace.Create();
        await File.WriteAllTextAsync(
            Path.Combine(workspace.Root, "visible.txt"),
            "VISIBLE_CONTENT",
            TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(
            workspace.OutsideFile,
            "SECRET_OUTSIDE_ROOT",
            TestContext.Current.CancellationToken);

        SafeFileStore store = new(workspace.Root);

        IReadOnlyList<string> matches = await store.SearchAsync(
            "SECRET_OUTSIDE_ROOT",
            TestContext.Current.CancellationToken);

        Assert.Empty(matches);
    }

    [Fact]
    public async Task WriteTextAsync_CreatesNestedFileInsideRoot()
    {
        using TempWorkspace workspace = TempWorkspace.Create();
        SafeFileStore store = new(workspace.Root);

        await store.WriteTextAsync("notes/today.txt", "inside", TestContext.Current.CancellationToken);

        Assert.Equal(
            "inside",
            await File.ReadAllTextAsync(
                Path.Combine(workspace.Root, "notes", "today.txt"),
                TestContext.Current.CancellationToken));
    }
}

internal sealed class SafeFileStore
{
    private readonly string _root;

    public SafeFileStore(string root)
    {
        this._root = Path.GetFullPath(root);
        Directory.CreateDirectory(this._root);
    }

    public async Task<string> ReadTextAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        string path = this.GetSafePath(relativePath);
        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public async Task WriteTextAsync(
        string relativePath,
        string text,
        CancellationToken cancellationToken = default)
    {
        string path = this.GetSafePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, text, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> SearchAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        List<string> matches = [];
        foreach (string path in Directory.EnumerateFiles(this._root, "*", SearchOption.AllDirectories))
        {
            string fullPath = this.GetSafePath(Path.GetRelativePath(this._root, path));
            string content = await File.ReadAllTextAsync(fullPath, cancellationToken);
            if (content.Contains(text, StringComparison.Ordinal))
            {
                matches.Add(Path.GetRelativePath(this._root, fullPath));
            }
        }

        return matches;
    }

    private string GetSafePath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            throw new ArgumentException("Path must be relative to the file-store root.", nameof(relativePath));
        }

        string fullPath = Path.GetFullPath(Path.Combine(this._root, relativePath));
        string rootWithSeparator = this._root.EndsWith(Path.DirectorySeparatorChar)
            ? this._root
            : this._root + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.Ordinal))
        {
            throw new ArgumentException("Path resolves outside the file-store root.", nameof(relativePath));
        }

        return fullPath;
    }
}

internal sealed class TempWorkspace : IDisposable
{
    private TempWorkspace(string parent, string root, string outsideFile)
    {
        this.Parent = parent;
        this.Root = root;
        this.OutsideFile = outsideFile;
    }

    public string Parent { get; }

    public string Root { get; }

    public string OutsideFile { get; }

    public static TempWorkspace Create()
    {
        string parent = Path.Combine(Path.GetTempPath(), "advanced-patterns", Guid.NewGuid().ToString("N"));
        string root = Path.Combine(parent, "root");
        string outsideFile = Path.Combine(parent, "outside.txt");

        Directory.CreateDirectory(root);
        return new TempWorkspace(parent, root, outsideFile);
    }

    public void Dispose()
    {
        if (Directory.Exists(this.Parent))
        {
            Directory.Delete(this.Parent, recursive: true);
        }
    }
}
