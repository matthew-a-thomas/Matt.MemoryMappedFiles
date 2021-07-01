namespace Matt.MemoryMappedFiles
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Disposes of more recently <see cref="Push"/>'d items first.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is not thread safe.
    /// </para>
    /// </remarks>
    sealed class StackDisposable : IDisposable
    {
        Stack<IDisposable>? _stack;

        public StackDisposable()
        {
            _stack = new Stack<IDisposable>();
        }

        /// <summary>
        /// Disposes of all the <see cref="Push"/>'d <see cref="IDisposable"/>s and causes any future ones to be
        /// disposed of immediately.
        /// </summary>
        public void Dispose()
        {
            var stack = Interlocked.Exchange(ref _stack, null);
            if (stack == null)
                return;
            var failures = new List<Exception>();
            while (stack.TryPop(out var disposable))
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception e)
                {
                    failures.Add(e);
                }
            }
            switch (failures.Count)
            {
                case 0:
                    return;
                case 1:
                    throw new Exception("There was an exception while disposing of the contained disposables", failures[0]);
                default:
                    throw new AggregateException("There were multiple exceptions while disposing of the contained disposables", failures);
            }
        }

        /// <summary>
        /// Adds the given <paramref name="disposable"/> to be disposed of later.
        /// </summary>
        /// <remarks>
        /// <para>
        /// More recently <see cref="Push"/>'d <see cref="IDisposable"/>s will be disposed of sooner.
        /// </para>
        /// <para>
        /// If this <see cref="StackDisposable"/> has already been disposed of then the given
        /// <paramref name="disposable"/> will immediately be disposed of.
        /// </para>
        /// </remarks>
        public void Push(IDisposable disposable)
        {
            var stack = _stack;
            if (stack == null)
            {
                disposable.Dispose();
                return;
            }
            stack.Push(disposable);
        }
    }
}