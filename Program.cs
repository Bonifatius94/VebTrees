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

            Console.Write("Initializing the queue ... ");

            // create lots of items to be inserted / deleted
            const ulong queueItemsCount = universeSize / 16;
            var items = Enumerable.Range(0, (int)queueItemsCount)
                .Select(x => ((ulong)rng.Next() % universeSize))
                .Distinct().ToHashSet();

            // initialize the tree
            var queue = new VebTree(universeBits);

            Console.WriteLine("Done!");
            Console.Write("Inserting items into the queue ... ");

            // insert all items into the vEB tree
            foreach (var item in items) { queue.Insert(item); }

            Console.WriteLine("Done!");
            Console.Write("Looking up inserted / missing items ... ");

            // ensure that the tree knows which items are inserted using Member()
            foreach (var item in items) {

                if (!queue.Member(item)) {
                    throw new Exception("Member() not working as expected!"); }
                if (queue.Member(item+1) != items.Contains(item+1)) {
                    throw new Exception("Member() not working as expected!"); }
            }

            Console.WriteLine("Done!");
            Console.Write("Sorting items using the queue like a linked list ... ");

            // use the tree to order items
            var sortedList = new List<ulong>();

            ulong? tempId = queue.GetMin();
            do { sortedList.Add(tempId.Value); }
            while ((tempId = queue.Successor(tempId.Value)) != null);

            if (!Enumerable.SequenceEqual(sortedList, items.OrderBy(x => x))) {
                throw new Exception("Successors don't have the right order!");
            }

            Console.WriteLine("Done!");
            Console.Write("Deleting items from the queue ... ");

            // delete all items from the queue
            foreach (var item in items) { queue.Delete(item); }

            // ensure that all items were actually deleted
            // note: calling member() on an empty queue is really slow
            foreach (var item in items) {
                if (queue.Member(item)) { throw new Exception(
                    "There are not properly deleted items!"); }
            }

            Console.WriteLine("Done!");
            Console.Write("Mixing all operations with collosions etc. ... ");

            // randomly lookup / insert / delete items
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

            Console.WriteLine("Done!");
        }
    }
}
