namespace Matt.MemoryMappedFiles.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using Xunit;

    public class MemoryMappedFileHelperClass
    {
        static IDisposable CreateTempFile(out string path)
        {
            var x = path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var fileStream = File.Open(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            return Disposable.Create(() =>
            {
                fileStream.Dispose();
                File.Delete(x);
            });
        }

        public class GetSpanProviderMethodShould
        {
            [Fact]
            public void FailToProvideWritableSpanForReadOnlyFile()
            {
                using var _ = CreateTempFile(out var path);
                using (var file = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    file.SetLength(1);
                using var readableFile = File.Open(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                );
                var helper = new MemoryMappedFileHelper();

                Assert.ThrowsAny<Exception>(() =>
                {
                    using var spanProvider = helper.GetSpanProvider(readableFile);
                });
            }

            [Fact]
            public void SupportReadingAndWritingToAFile()
            {
                using var _ = CreateTempFile(out var path);
                using var file = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                file.SetLength(1);
                var helper = new MemoryMappedFileHelper();
                using var spanProvider = helper.GetSpanProvider(file);
                var span = spanProvider.GetSpan(0, 1);

                for (var i = 0; i < 256; ++i)
                {
                    span[0] = (byte)i;
                    file.Position = 0;
                    Assert.Equal(i, file.ReadByte());

                    file.Position = 0;
                    file.WriteByte(0xa5);
                    file.Flush();
                    Assert.Equal(0xa5, span[0]);
                }
            }
        }

        public class GetReadOnlySpanProviderMethodShould
        {
            [Fact]
            public void SuccessfullyReadDataFromReadOnlyFile()
            {
                using var _ = CreateTempFile(out var path);
                using (var file = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                    file.Write(Encoding.UTF8.GetBytes("Hello, world!"));
                }
                using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var helper = new MemoryMappedFileHelper();

                    using var spanProvider = helper.GetReadOnlySpanProvider(file);
                    var span = spanProvider.GetReadOnlySpan(0, 13);
                    var s = Encoding.UTF8.GetString(span);

                    Assert.Equal("Hello, world!", s);
                }
            }
        }
    }
}