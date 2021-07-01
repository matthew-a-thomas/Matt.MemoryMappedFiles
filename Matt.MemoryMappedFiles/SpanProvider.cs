namespace Matt.MemoryMappedFiles
{
    using System;

    /// <summary>
    /// Provides writable <see cref="Span{T}"/>s at potentially very large offsets from a pointer.
    /// </summary>
    public sealed class SpanProvider : IDisposable
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

        /// <summary>
        /// Returns a <see cref="Span{T}"/> that starts at the given <paramref name="offset"/> and covers
        /// <paramref name="length"/> bytes.
        /// </summary>
        public Span<byte> GetSpan(
            long offset,
            int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (offset + length > _length)
                throw new ArgumentOutOfRangeException(nameof(length));
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