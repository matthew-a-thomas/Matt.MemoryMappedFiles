namespace Matt.MemoryMappedFiles
{
    using System;

    /// <summary>
    /// Provides read-only <see cref="ReadOnlySpan{T}"/>s at potentially very large offsets from a pointer.
    /// </summary>
    public sealed class ReadOnlySpanProvider : IDisposable
    {
        readonly unsafe byte* _baseAddress;
        readonly IDisposable _disposable;
        readonly long _length;

        public unsafe ReadOnlySpanProvider(
            byte* baseAddress,
            IDisposable disposable,
            long length)
        {
            _baseAddress = baseAddress;
            _disposable = disposable;
            _length = length;
        }

        /// <summary>
        /// Returns a <see cref="ReadOnlySpan{T}"/> that starts at the given <paramref name="offset"/> and covers
        /// <paramref name="length"/> bytes.
        /// </summary>
        public ReadOnlySpan<byte> GetReadOnlySpan(
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