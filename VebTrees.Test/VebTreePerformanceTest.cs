using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Xunit;

namespace VebTrees.Test
{
    public class VebTreePerformanceTestXunit
    {
        [Fact]
        public void PerformanceTest()
        {
            // info: this is just a dummy to launch the benchmark
            //       without having to write a main function
            BenchmarkRunner.Run<VebTreeInitPerformanceTest>();
            BenchmarkRunner.Run<VebTreeInsertPerformanceTest>();
            BenchmarkRunner.Run<VebTreeDeletePerformanceTest>();
            BenchmarkRunner.Run<VebTreeInsertAndDeletePerformanceTest>();
            BenchmarkRunner.Run<VebTreeSortPerformanceTest>();
            // BenchmarkRunner.Run<VebTreeMixedOpsPerformanceTest>();
        }
    }

    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50, runStrategy: RunStrategy.ColdStart, launchCount: 1, targetCount: 5)]
    public class VebTreeInitPerformanceTest
    {
        // create a seeded random number generator (for test repeatability)
        private static readonly Random rng = new Random(0);

        // define the universe sizes to be tested
        // [Params(1, 10, 15, 20, 25, 32, 42, 55, 64)]
        [Params(1, 10, 15, 20, 25)]
        public byte universeBits;

        private VebTree queue;

        [Benchmark]
        public void VebInitTest()
        {
            queue = new VebTree(universeBits);
            Assert.True(queue.IsEmpty());
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            queue = null;
            GC.Collect();
        }
    }

    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50, runStrategy: RunStrategy.ColdStart, launchCount: 1, targetCount: 5)]
    public class VebTreeInsertPerformanceTest
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
        }

        [IterationSetup]
        public void PreTest()
        {
            queue = new VebTree(universeBits);
        }

        [Benchmark]
        public void VebInsertTest()
        {
            Assert.True(queue.IsEmpty());
            foreach (var item in items) { queue.Insert(item); }
            Assert.True(!queue.IsEmpty());
        }

        [IterationCleanup]
        public void PostTest()
        {
            queue = null;
            GC.Collect();
        }
    }

    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50, runStrategy: RunStrategy.ColdStart, launchCount: 1, targetCount: 5)]
    public class VebTreeDeletePerformanceTest
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
        }

        [IterationSetup]
        public void PreTest()
        {
            queue = new VebTree(universeBits);
            foreach (var item in items) { queue.Insert(item); }
        }

        [Benchmark]
        public void VebDeleteTest()
        {
            foreach (var item in items) { queue.Delete(item); }
            Assert.True(queue.IsEmpty());
        }

        [IterationCleanup]
        public void PostTest()
        {
            queue = null;
            GC.Collect();
        }
    }

    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50, runStrategy: RunStrategy.ColdStart, launchCount: 1, targetCount: 5)]
    public class VebTreeInsertAndDeletePerformanceTest
    {
        // create a seeded random number generator (for test repeatability)
        private static readonly Random rng = new Random(0);

        // define the universe sizes to be tested
        [Params(10, 15, 20)]
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
            queueItemsCount = universeSize / 2;
            items = Enumerable.Range(0, (int)queueItemsCount)
                .Select(x => ((ulong)rng.Next() % universeSize))
                .Distinct().ToHashSet();
        }

        [IterationSetup]
        public void PreTest()
        {
            queue = new VebTree(universeBits);
        }

        [Benchmark]
        public void VebInsertAndDeleteTest()
        {
            
            Assert.True(queue.IsEmpty());
            foreach (var item in items) { queue.Insert(item); }
            foreach (var item in items) { queue.Delete(item); }
            Assert.True(queue.IsEmpty());
        }

        [IterationCleanup]
        public void PostTest()
        {
            queue = null;
            GC.Collect();
        }
    }

    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50, runStrategy: RunStrategy.ColdStart, launchCount: 1, targetCount: 5)]
    public class VebTreeSortPerformanceTest
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

        private List<ulong> vebSortedItems;
        private List<ulong> qsSortedItems;

        [GlobalSetup]
        public void Setup()
        {
            universeSize = (ulong)1 << universeBits;
            queueItemsCount = universeSize / 16;
            items = Enumerable.Range(0, (int)queueItemsCount)
                .Select(x => ((ulong)rng.Next() % universeSize))
                .Distinct().ToHashSet();

            VebSortTest();
            QuicksortTest();
        }

        [Benchmark]
        public void VebSortTest()
        {
            queue = new VebTree(universeBits);
            foreach (var item in items) { queue.Insert(item); }

            vebSortedItems = new List<ulong>();
            ulong? tempMin = queue.GetMin();

            do { vebSortedItems.Add(tempMin.Value); }
            while ((tempMin = queue.Successor(tempMin.Value)) != null);
        }

        [Benchmark(Baseline = true)]
        public void QuicksortTest()
        {
            qsSortedItems = items.OrderBy(x => x).ToList();
        }

        [IterationCleanup]
        public void PostTest()
        {
            Assert.True(Enumerable.SequenceEqual(vebSortedItems, qsSortedItems));
            queue = null;
            GC.Collect();
        }
    }

    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50, runStrategy: RunStrategy.ColdStart, launchCount: 1, targetCount: 5)]
    public class VebTreeMixedOpsPerformanceTest
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
        public void MixedOpsTest()
        {
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
