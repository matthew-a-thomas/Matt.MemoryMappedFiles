namespace Matt.MemoryMappedFiles
{
    using System;
    using System.IO;
    using System.IO.MemoryMappedFiles;

    /// <summary>
    /// Helps expose memory mapped files as <see cref="Span{T}"/>s.
    /// </summary>
    public static class MemoryMappedFileHelper
    {
        static unsafe byte* AcquirePointer(
            FileStream stream,
            MemoryMappedFileAccess access,
            out IDisposable disposable)
        {
            var stackDisposable = new StackDisposable();
            disposable = stackDisposable;
            try
            {
                var memoryMappedFile = MemoryMappedFile.CreateFromFile(
                    fileStream: stream,
                    mapName: null,
                    capacity: 0,
                    access: access,
                    inheritability: HandleInheritability.None,
                    leaveOpen: true);
                stackDisposable.Push(memoryMappedFile);
                var viewAccessor = memoryMappedFile.CreateViewAccessor(
                    offset: 0,
                    size: 0,
                    access: access);
                stackDisposable.Push(viewAccessor);
                var handle = viewAccessor.SafeMemoryMappedViewHandle;
                byte* pointer = default;
                handle.AcquirePointer(ref pointer);
                stackDisposable.Push(Disposable.Create(handle.ReleasePointer));
                return pointer;
            }
            catch
            {
                stackDisposable.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a <see cref="SpanProvider"/> which can create writable <see cref="Span{T}"/>s from the given
        /// <see cref="FileStream"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The given <see cref="FileStream"/> must support writing or an exception will be thrown. The file must also
        /// have a non-zero size.
        /// </para>
        /// </remarks>
        public static SpanProvider CreateSpanProvider(FileStream stream)
        {
            unsafe
            {
                var pointer = AcquirePointer(
                    stream: stream,
                    access: MemoryMappedFileAccess.ReadWrite,
                    out var disposable);
                return new SpanProvider(
                    baseAddress: pointer,
                    disposable: disposable,
                    length: stream.Length
                );
            }
        }

        /// <summary>
        /// Creates a <see cref="ReadOnlySpanProvider"/> which can create read-only <see cref="ReadOnlySpan{T}"/>s from
        /// the given <see cref="FileStream"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The given <see cref="FileStream"/> must have a non-zero size or an exception will be thrown.
        /// </para>
        /// </remarks>
        public static ReadOnlySpanProvider CreateReadOnlySpanProvider(FileStream stream)
        {
            unsafe
            {
                var pointer = AcquirePointer(
                    stream: stream,
                    access: MemoryMappedFileAccess.Read,
                    out var disposable);
                return new ReadOnlySpanProvider(
                    baseAddress: pointer,
                    disposable: disposable,
                    length: stream.Length
                );
            }
        }
    }
}