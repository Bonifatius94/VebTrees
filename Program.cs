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
            var itemsToInsert = Enumerable.Range(0, (int)queueItemsCount)
                .Select(x => ((ulong)rng.Next() % universeSize))
                .Distinct().ToHashSet();

            // initialize the tree
            var queue = new VebTreeNode(universeBits);

            Console.WriteLine("Done!");
            Console.Write("Inserting items into the queue ... ");

            // insert all items into the vEB tree
            foreach (var item in itemsToInsert) { queue.Insert(item); }

            Console.WriteLine("Done!");
            Console.Write("Looking up inserted / missing items ... ");

            // ensure that the tree knows which items are inserted using Member()
            foreach (var item in itemsToInsert) {

                if (!queue.Member(item)) {
                    throw new Exception("Member() not working as expected!"); }
                if (queue.Member(item+1) != itemsToInsert.Contains(item+1)) {
                    throw new Exception("Member() not working as expected!"); }
            }

            Console.WriteLine("Done!");
            Console.Write("Sorting items using the queue like a linked list ... ");

            // use the tree to order items
            var sortedList = new List<ulong>();

            ulong? tempId = queue.GetMin();
            do { sortedList.Add(tempId.Value); }
            while ((tempId = queue.Successor(tempId.Value)) != null);

            if (!Enumerable.SequenceEqual(sortedList, itemsToInsert.OrderBy(x => x))) {
                throw new Exception("Successors don't have the right order!");
            }

            Console.WriteLine("Done!");
            Console.Write("Deleting items from the queue ... ");

            // delete all items from the queue
            foreach (var item in itemsToInsert) { queue.Delete(item); }

            Console.WriteLine("Done!");

            // ensure that all items were actually deleted
            // note: calling member() on an empty queue is really slow
            foreach (var item in itemsToInsert) {
                if (queue.Member(item)) { throw new Exception(
                    "There are not properly deleted items!"); }
            }
        }
    }
}
