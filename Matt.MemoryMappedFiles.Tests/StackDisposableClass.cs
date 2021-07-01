namespace Matt.MemoryMappedFiles.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Moq;
    using Xunit;

    [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
    public class StackDisposableClass
    {
        [Fact]
        public void ShouldDisposeExactlyOnce()
        {
            var disposable = new StackDisposable();
            var count = 0;
            var mockDisposable = new Mock<IDisposable>();
            mockDisposable.Setup(x => x.Dispose()).Callback(() => Interlocked.Increment(ref count));
            disposable.Push(mockDisposable.Object);

            Assert.Equal(0, count);
            disposable.Dispose();
            Assert.Equal(1, count);
            disposable.Dispose();
            Assert.Equal(1, count);
        }

        [Fact]
        public void ShouldDisposeInReverseOrder()
        {
            var disposable = new StackDisposable();
            var order = new List<int>();
            disposable.Push(Disposable.Create(() => order.Add(0)));
            disposable.Push(Disposable.Create(() => order.Add(1)));

            disposable.Dispose();

            Assert.Equal<int>(new [] { 1, 0 }, order);
        }
    }
}