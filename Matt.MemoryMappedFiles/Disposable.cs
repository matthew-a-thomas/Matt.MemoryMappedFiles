namespace Matt.MemoryMappedFiles
{
    using System;
    using System.Threading;

    public sealed class Disposable : IDisposable
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