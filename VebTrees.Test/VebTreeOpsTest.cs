using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VebTrees.Test
{
    public class VebTreeOpsTest
    {
        [Fact]
        public void InitTest()
        {
            var queue = VebTreeFactory.CreateTree(1);
            Assert.True(queue.IsEmpty());
            queue = VebTreeFactory.CreateTree(15);
            Assert.True(queue.IsEmpty());
            queue = VebTreeFactory.CreateTree(32);
            Assert.True(queue.IsEmpty());
            // queue = VebTreeFactory.CreateTree(64); // TODO: support 2^64 sized universes
            // Assert.True(queue.IsEmpty());
        }

        [Fact]
        public void InsertAndDeleteSingleTest()
        {
            byte minUniverseBits = 2;
            byte maxUniverseBits = 10;
            for (byte u = minUniverseBits; u < maxUniverseBits; u++)
            {
                for (ulong i = 0; i < ((ulong)1 << u); i++)
                {
                    var queue = VebTreeFactory.CreateTree(u);
                    queue.Insert(i);
                    Assert.True(queue.Member(i));
                    queue.Delete(i);
                    Assert.True(!queue.Member(i) && queue.IsEmpty());
                }
            }
        }

        [Fact]
        public void InsertAndDeleteFullUniverseTest()
        {
            byte minUniverseBits = 2;
            byte maxUniverseBits = 10;
            for (byte u = minUniverseBits; u < maxUniverseBits; u++)
            {
                var queue = VebTreeFactory.CreateTree(u);
                for (ulong i = 0; i < ((ulong)1 << u); i++) { queue.Insert(i); }
                for (ulong i = 0; i < ((ulong)1 << u); i++) { Assert.True(queue.Member(i)); }
                for (ulong i = 0; i < ((ulong)1 << u); i++) { queue.Delete(i); }
                for (ulong i = 0; i < ((ulong)1 << u); i++) { Assert.True(!queue.Member(i)); }
                Assert.True(queue.IsEmpty());
            }
        }

        [Fact]
        public void GracefulDuplicateInsertsAndDeletesTest()
        {
            // TODO: do this test also for a universe size <= 64 (this tests only the common vEB tree)
            var queue = VebTreeFactory.CreateTree(10);
            queue.Insert(10);
            queue.Insert(10);
            Assert.True(queue.Member(10) && !queue.IsEmpty());
            queue.Delete(10);
            queue.Delete(10);
            Assert.True(!queue.Member(10) && queue.IsEmpty());
        }

        // [Fact]
        // public void InsertValueOverflowExceptionTest()
        // {
            // TODO: check if the program acutally throws an exception
            //       -> if not, add code for throwing exceptions to make this test pass
            // var queue = VebTreeFactory.CreateTree(2);
            // Assert.Throws<ArgumentException>(() => queue.Insert(4));
        // }

        [Fact]
        public void SuccessorTest()
        {
            // TODO: do this test also for a universe size > 64 (this tests only the bitwise tree leaf)
            var queue = VebTreeFactory.CreateTree(2);
            Assert.True(queue.Successor(0) == null);
            queue.Insert(0);
            Assert.True(queue.Successor(0) == null);
            queue.Insert(1);
            Assert.True(queue.Successor(0) == 1);
            queue.Insert(2);
            Assert.True(queue.Successor(0) == 1);
            queue.Insert(3);
            Assert.True(queue.Successor(0) == 1);
            queue.Delete(1);
            Assert.True(queue.Successor(0) == 2);
            queue.Delete(2);
            Assert.True(queue.Successor(0) == 3);
            queue.Delete(3);
            Assert.True(queue.Successor(0) == null);
            queue.Delete(0);
            Assert.True(queue.Successor(0) == null);
            queue.Insert(0);
            Assert.True(queue.Successor(2) == null);
        }
    }
}
