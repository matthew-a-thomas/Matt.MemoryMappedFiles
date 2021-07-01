namespace Matt.MemoryMappedFiles.Tests
{
    using System.Threading;
    using Xunit;

    public class DisposableClass
    {
        [Fact]
        public void ShouldDisposeExactlyOnce()
        {
            var count = 0;
            var disposable = Disposable.Create(() => Interlocked.Increment(ref count));

            Assert.Equal(0, count);
            disposable.Dispose();
            Assert.Equal(1, count);
            disposable.Dispose();
            Assert.Equal(1, count);
        }
    }
}