using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace UnitTests;

public class Lifecycle
{
    private static readonly Stopwatch SessionTimer = new();

    [Before(TestSession)]
    public static Task StartSession(TestSessionContext _)
    {
        SessionTimer.Start();
        return Task.CompletedTask;
    }

    [After(TestSession)]
    public static Task EndSession(TestSessionContext _)
    {
        SessionTimer.Stop();
        return Task.CompletedTask;
    }
}
