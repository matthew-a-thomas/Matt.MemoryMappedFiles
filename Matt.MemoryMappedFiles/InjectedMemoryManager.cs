namespace Matt.MemoryMappedFiles
{
    using System;
    using System.Buffers;

    sealed class InjectedMemoryManager<T> : MemoryManager<T>
    {
        public delegate Span<T> GetSpanDelegate();

        readonly Action _dispose;
        readonly GetSpanDelegate _getSpan;
        readonly Func<int, MemoryHandle> _pin;
        readonly Action _unpin;

        public InjectedMemoryManager(
            GetSpanDelegate getSpan,
            Action? dispose = null,
            Func<int, MemoryHandle>? pin = null,
            Action? unpin = null)
        {
            _dispose = dispose ?? NullDispose;
            _getSpan = getSpan;
            _pin = pin ?? NullPin;
            _unpin = unpin ?? NullUnpin;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _dispose();
        }

        public override Span<T> GetSpan() => _getSpan();

        static void NullDispose() {}

        static MemoryHandle NullPin(int _) => default;

        static void NullUnpin() {}

        public override MemoryHandle Pin(int elementIndex = 0) => _pin(elementIndex);

        public override void Unpin() => _unpin();
    }
}