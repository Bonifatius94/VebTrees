using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Xunit;

namespace VebTrees.Test
{
    public class VebTreeOpsTest
    {
        [Fact]
        public void InitTest()
        {
            var queue = new VebTree(1);
            queue = new VebTree(15);
            queue = new VebTree(32);
            queue = new VebTree(64);
            Assert.True(queue.IsEmpty());
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
                    var queue = new VebTree(u);
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
                var queue = new VebTree(u);
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
            var queue = new VebTree(10);
            queue.Insert(10);
            queue.Insert(10);
            Assert.True(queue.Member(10) && !queue.IsEmpty());
            queue.Delete(10);
            queue.Delete(10);
            Assert.True(!queue.Member(10) && queue.IsEmpty());
        }

        [Fact]
        public void InsertValueOverflowExceptionTest()
        {
            // TODO: check if the program acutally throws an exception
            //       -> if not, add code for throwing exceptions to make this test pass
            var queue = new VebTree(2);
            Assert.Throws<ArgumentException>(() => queue.Insert(4));
        }

        [Fact]
        public void SuccessorTest()
        {
            var queue = new VebTree(2);
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
        }
    }
}
