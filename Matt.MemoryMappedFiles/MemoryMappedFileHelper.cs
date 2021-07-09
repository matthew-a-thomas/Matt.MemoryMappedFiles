namespace Matt.MemoryMappedFiles
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.IO.MemoryMappedFiles;

    /// <summary>
    /// Helps expose memory mapped files as <see cref="Memory{T}"/>s.
    /// </summary>
    public static class MemoryMappedFileHelper
    {
        static unsafe byte* AcquirePointer(
            FileStream stream,
            MemoryMappedFileAccess access,
            bool keepOpen,
            out StackDisposable disposable)
        {
            var stackDisposable = new StackDisposable();
            disposable = stackDisposable;
            try
            {
                if (!keepOpen)
                    stackDisposable.Push(stream);
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
            catch (Exception e)
            {
                stackDisposable.Dispose();
                throw new Exception("Failed to acquire a pointer to the memory mapped file", e);
            }
        }

        /// <summary>
        /// Creates a <see cref="MemoryManager{T}"/> that exposes <see cref="Memory{T}"/> covering the indicated range
        /// of the given <see cref="FileStream"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// You have to be careful that read/write access aligns with the permissions of the given
        /// <see cref="FileStream"/>, the given <paramref name="access"/> parameter, and how the <see cref="Memory{T}"/>
        /// is used in the returned <see cref="MemoryManager{T}"/>.
        /// </para>
        /// <para>
        /// Disposing of the returned <see cref="MemoryManager{T}"/> will dispose of the given <see cref="FileStream"/>
        /// if <paramref name="keepOpen"/> is false.
        /// </para>
        /// </remarks>
        public static unsafe MemoryManager<byte> CreateMemoryManager(
            FileStream fileStream,
            MemoryMappedFileAccess access,
            long offset = 0,
            int? length = null,
            bool keepOpen = false)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            var fileLength = fileStream.Length;
            length ??= (int) Math.Min(int.MaxValue, fileLength - offset);
            if (offset + length > fileLength)
                throw new ArgumentOutOfRangeException(nameof(length));
            var baseAddress = AcquirePointer(
                fileStream,
                access,
                keepOpen,
                out var disposable);
            return new InjectedMemoryManager<byte>(
                () => new Span<byte>(
                    pointer: baseAddress + offset,
                    length: length.Value
                ),
                dispose: disposable.Dispose
            );
        }
    }
}