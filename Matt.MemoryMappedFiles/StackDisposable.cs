namespace Matt.MemoryMappedFiles
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class StackDisposable : IDisposable
    {
        Stack<IDisposable>? _stack;

        public StackDisposable()
        {
            _stack = new Stack<IDisposable>();
        }

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