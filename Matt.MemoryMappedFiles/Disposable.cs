namespace Matt.MemoryMappedFiles
{
    using System;
    using System.Threading;

    /// <summary>
    /// Invokes an injected <see cref="Action"/> no more than once when disposed of.
    /// </summary>
    sealed class Disposable : IDisposable
    {
        Action? _dispose;

        Disposable(Action dispose)
        {
            _dispose = dispose;
        }

        public static Disposable Create(Action dispose) => new(dispose);

        public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}