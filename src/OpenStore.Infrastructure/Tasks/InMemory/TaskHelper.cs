namespace OpenStore.Infrastructure.Tasks.InMemory;

public static class TaskHelper
{
    /// <summary>
    /// Runs a TPL Task fire-and-forget style, the right way - in the
    /// background, separate from the current thread, with no risk
    /// of it trying to rejoin the current thread.
    /// </summary>
    public static void RunBg(Func<Task> fn, CancellationToken token)
    {
        Task.Run(fn, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs a task fire-and-forget style and notifies the TPL that this
    /// will not need a Thread to resume on for a long time, or that there
    /// are multiple gaps in thread use that may be long.
    /// Use for example when talking to a slow webservice.
    /// </summary>
    public static Task RunBgLong(Func<Task> fn)
    {
        var t = Task.Factory.StartNew(fn, TaskCreationOptions.LongRunning);
        t.ConfigureAwait(false);
        return t;
    }
}