namespace Matt.MemoryMappedFiles
{
    using System;
    using System.IO;
    using System.IO.MemoryMappedFiles;

    public class MemoryMappedFileHelper
    {
        unsafe byte* AcquirePointer(
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

        public SpanProvider GetSpanProvider(
            FileStream stream)
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

        public ReadOnlySpanProvider GetReadOnlySpanProvider(
            FileStream stream)
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