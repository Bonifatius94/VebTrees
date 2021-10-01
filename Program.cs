using System;
using System.Collections.Generic;
using System.Linq;

namespace VebTrees
{
    public class Program
    {
        private static readonly Random rng = new Random(0);

        public static void Main(string[] args)
        {
            // define the universe size to be tested
            const byte universeBits = 24;
            const ulong universeSize = (ulong)1 << universeBits;

            // create lots of items to be inserted / deleted
            const ulong queueItemsCount = universeSize / 16;
            var items = Enumerable.Range(0, (int)queueItemsCount)
                .Select(x => ((ulong)rng.Next() % universeSize))
                .Distinct().ToHashSet();

            Console.WriteLine($"Testing with { items.Count } random items.");
            Console.Write("Initializing the queue (lazy init) ... ");

            // initialize the tree
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var queue = new VebTree(universeBits);
            watch.Stop();

            Console.WriteLine($"Done, took { watch.ElapsedMilliseconds } ms!");
            Console.Write("Inserting items into the queue ... ");

            // insert all items into the vEB tree
            watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var item in items) { queue.Insert(item); }
            watch.Stop();

            Console.WriteLine($"Done, took { watch.ElapsedMilliseconds } ms for { items.Count } ops, allocated ~ { GC.GetTotalMemory(false) } bytes!");
            Console.Write("Looking up inserted / missing items ... ");

            // ensure that the tree knows which items are inserted using Member()
            watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var item in items) {

                if (!queue.Member(item)) {
                    throw new Exception("Member() not working as expected!"); }
                if (queue.Member(item+1) != items.Contains(item+1)) {
                    throw new Exception("Member() not working as expected!"); }
            }
            watch.Stop();

            Console.WriteLine($"Done, took { watch.ElapsedMilliseconds } ms for { items.Count * 2 } ops!");
            Console.Write("Sorting items using the queue like a linked list ... ");

            // use the already existing tree to sort items
            watch = System.Diagnostics.Stopwatch.StartNew();
            var sortedList = new List<ulong>();
            ulong? tempMin = queue.GetMin();
            do { sortedList.Add(tempMin.Value); }
            while ((tempMin = queue.Successor(tempMin.Value)) != null);
            watch.Stop();

            // sort the items using quick sort and make sure the result is the same
            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            var expOrderedList = items.OrderBy(x => x).ToList();
            watch2.Stop();
            if (!Enumerable.SequenceEqual(sortedList, expOrderedList)) {
                throw new Exception("Successors don't have the right order!");
            }

            Console.WriteLine($"Done, took { watch.ElapsedMilliseconds } ms for { items.Count } items "
                + $"(given the items were already inserted, quicksort took { watch2.ElapsedMilliseconds } ms)!");
            Console.Write("Deleting items from the queue ... ");

            // delete all items from the queue
            watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var item in items) { queue.Delete(item); }
            watch.Stop();

            // ensure that all items were actually deleted
            // note: calling member() on an empty queue is really slow
            foreach (var item in items) {
                if (queue.Member(item)) { throw new Exception(
                    "There are not properly deleted items!"); }
            }

            Console.WriteLine($"Done, took { watch.ElapsedMilliseconds } ms for { items.Count } ops!");
            Console.Write("Mixing all operations with collosions etc. ... ");

            // randomly lookup / insert / delete items
            watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < items.Count * 4; i++)
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
            watch.Stop();
            Console.WriteLine($"Done, took { watch.ElapsedMilliseconds } ms for { items.Count * 4 } ops!");
        }
    }
}
