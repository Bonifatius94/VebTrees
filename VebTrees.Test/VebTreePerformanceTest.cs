using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Xunit;

namespace VebTrees.Test
{
    public class VebTreePerformanceTest
    {
        // create a seeded random number generator (for test repeatability)
        private static readonly Random rng = new Random(0);

        // define the universe sizes to be tested
        [Params(10, 15, 20, 25)]
        public byte universeBits;

        // create lots of items to be inserted / deleted
        private ulong universeSize;
        private ulong queueItemsCount;
        private HashSet<ulong> items;
        private VebTree queue;

        [GlobalSetup]
        public void Setup()
        {
            universeSize = (ulong)1 << universeBits;
            queueItemsCount = universeSize / 16;
            items = Enumerable.Range(0, (int)queueItemsCount)
                .Select(x => ((ulong)rng.Next() % universeSize))
                .Distinct().ToHashSet();

            queue = new VebTree(universeBits);
            foreach (var item in items) { queue.Insert(item); }
        }

        [Benchmark]
        public void VebInitTest()
        {
            var queue1 = new VebTree(universeBits);
            Assert.True(queue1.IsEmpty());
        }

        [Benchmark]
        public void VebInsertTest()
        {
            var queue2 = new VebTree(universeBits);
            Assert.True(queue2.IsEmpty());

            foreach (var item in items) { queue2.Insert(item); }
            Assert.True(!queue2.IsEmpty());
        }

        [Benchmark]
        public void VebMemberTest()
        {
            foreach (var item in items) { Assert.True(queue.Member(item)); }
        }

        [Benchmark]
        public void VebSortTest()
        {
            // use the already existing tree to sort items
            var sortedList = new List<ulong>();
            ulong? tempMin = queue.GetMin();
            do { sortedList.Add(tempMin.Value); }
            while ((tempMin = queue.Successor(tempMin.Value)) != null);

            // sort the items using quick sort and make sure the result is the same
            var expOrderedList = items.OrderBy(x => x).ToList();
            Assert.True(Enumerable.SequenceEqual(sortedList, expOrderedList));
        }

        [Benchmark]
        public void VebDeleteTest()
        {
            foreach (var item in items) { queue.Delete(item); }
            Assert.True(queue.IsEmpty());
        }

        [Benchmark]
        public void BenchmarkTest()
        {
            var queue = new VebTree(universeBits);

            for (int i = 0; i < items.Count * 20; i++)
            {
                ulong id = (ulong)rng.Next() % universeSize;
                int opType = rng.Next() % 7;

                switch (opType)
                {
                    case 0: queue.Insert(id); break;
                    case 1: queue.Delete(id); break;
                    case 2: queue.Member(id); break;
                    case 3: queue.Successor(id); break;
                    case 4: queue.IsEmpty(); break;
                    case 5: queue.GetMin(); break;
                    case 6: queue.GetMax(); break;
                }
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            queue = null;
            GC.Collect();
        }
    }
}
