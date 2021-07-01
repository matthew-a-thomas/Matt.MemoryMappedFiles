namespace Matt.MemoryMappedFiles
{
    using System;

    public class SpanProvider : IDisposable
    {
        readonly unsafe byte* _baseAddress;
        readonly IDisposable _disposable;
        readonly long _length;

        public unsafe SpanProvider(
            byte* baseAddress,
            IDisposable disposable,
            long length)
        {
            _baseAddress = baseAddress;
            _disposable = disposable;
            _length = length;
        }

        public Span<byte> GetSpan(
            long offset,
            int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (offset + length > _length)
                throw new ArgumentOutOfRangeException();
            unsafe
            {
                return new Span<byte>(
                    pointer: _baseAddress + offset,
                    length: length
                );
            }
        }

        public void Dispose() => _disposable.Dispose();
    }
}