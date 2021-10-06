/*
MIT License
Copyright (c) 2021 Marco Tröster
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE add a feature to OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Numerics;

namespace VebTrees
{
    /// <summary>
    /// An implementation of the van-Emde-Boas tree data structure that can be used
    /// as a priority queue supporting all operations with at most O(log log u) time.
    /// </summary>
    public class VebTree : IPriorityQueue
    {
        // info: this class is wrapping up the van-Emde-Boas tree to catch invalid
        //       insertions / deletions. It's meant to be used by vEB library users.

        /// <summary>
        /// Create a new instance of a van-Emda-Boas tree with the given universe size.
        /// </summary>
        /// <param name="universeBits">The universe size as bits.</param>
        public VebTree(byte universeBits) { root = new VebTreeNode(universeBits); }
        // TODO: enable/disable lazy-loading by a setting

        private VebTreeNode root;

        public bool IsEmpty() => root.IsEmpty();
        public ulong? GetMin() => root.GetMin();
        public ulong? GetMax() => root.GetMax();
        public bool Member(ulong id) => !root.IsEmpty() && root.Member(id);
        public ulong? Successor(ulong id) => root.IsEmpty() ? null : root.Successor(id);
        public ulong? Predecessor(ulong id) => root.IsEmpty() ? null : root.Predecessor(id);
        public void Insert(ulong id) { if (!Member(id)) { root.Insert(id); } }
        public void Delete(ulong id) { if (Member(id)) { root.Delete(id); } }
    }

    // TODO: use a ulong bitboard for nodes with u <= 64 (saves 2 recursions and lots of bits)

    /// <summary>
    /// An implementation of the van-Emde-Boas tree data structure that can be used
    /// as a priority queue supporting all operations with at most O(log log u) time.
    /// </summary>
    internal class VebTreeNode : IPriorityQueue
    {
        public VebTreeNode(byte universeBits)
        {
            // assign universe bits
            this.universeBits = universeBits;

            // initialize children nodes (local / global)
            global = null;
            local = new VebTreeNode[m];

            // initialize routing attributes
            low = null; high = null;
        }

        // tree node parameters
        private readonly byte universeBits;

        private ulong m => (ulong)1 << upperBits;
        private byte upperBits => (byte)(universeBits - lowerBits);
        private byte lowerBits => (byte)(universeBits / 2);

        // tree node content
        private ulong? low;
        private ulong? high;
        private VebTreeNode global;
        private VebTreeNode[] local;

        public bool IsEmpty() => low == null;
        public ulong? GetMin() => low;
        public ulong? GetMax() => high;

        public bool Member(ulong id)
            => id >= low && id <= high && Successor(id - 1) == id;

        public ulong? Successor(ulong id)
        {
            // base case for universe with { 0, 1 }
            // when looking up the 0, check if the 1 is present in the structure as well
            if (universeBits == 1) { return (id == 0 && high.Value == 1) ? (ulong)1 : null; }

            // case when the predecessor is in the neighbour structure -> low is the successor
            if (low != null && id < low) { return low; }

            // case when the id needs to be looked up in a child node

            ulong upper = upperAddress(id);
            ulong lower = lowerAddress(id);
            local[upper] = local[upper] ?? new VebTreeNode(lowerBits);

            // subcase 1: id's successor is in the same child as the id itself
            var localChild = local[upper];
            ulong? localMax = localChild?.GetMax();
            if (localMax != null && lower < localMax) {
                return (upper << lowerBits) | (localChild.Successor(lower));
            }

            // subcase 2: id's successor is in a successing child node,
            //            defaulting to null if there's no successor
            global = global ?? new VebTreeNode(upperBits);
            ulong? succ = global.Successor(upperAddress(id));
            return succ != null ? ((succ.Value << lowerBits) | local[succ.Value].GetMin()) : null;
        }

        public ulong? Predecessor(ulong id)
        {
            // TODO: implement analog to the successor function
            throw new NotImplementedException();
        }

        public void Insert(ulong id)
        {
            // base case when low is null -> set low/high to id
            if (low == null) { low = high = id; return; }

            // case when there is a minimum, but the id to insert is smaller
            // -> swap low with id -> insert low instead of the id
            if (id < low) { ulong temp = low.Value; low = id; id = temp; }

            // make sure it's not the base case for universe { 0, 1 }
            if (universeBits > 1) {

                // cache lower / upper address parts
                ulong upper = upperAddress(id);
                ulong lower = lowerAddress(id);
                local[upper] = local[upper] ?? new VebTreeNode(lowerBits);

                // mark sure to update global when inserting into an empty child node
                if (local[upper].IsEmpty()) {
                    global = global ?? new VebTreeNode(upperBits);
                    global.Insert(upper);
                }

                // insert the node into the child node
                // takes O(1) when inserting into an empty child node
                local[upper].Insert(lower);
            }

            // update the maximum pointer
            high = Math.Max(high.Value, id);
        }

        public void Delete(ulong id)
        {
            // base case with only one element -> set low and high to null
            if (low == high) {
                low = high = null;
                global = new VebTreeNode(upperBits);
                return;
            }

            // base case with universe { 0, 1 } -> flip the bit
            if (universeBits == 1) { low = high = 1 - id; return; }

            // case when deleting the minimum
            if (id == low) {

                if (global.low == null) { throw new InvalidOperationException(
                    "global.low is null, this should never happen..."); }

                // find the new minimum in the children nodes
                // -> delete the new minimum from the child node
                ulong i = global.low.Value;
                ulong newMin = (i << lowerBits) | (ulong)local[i].GetMin();
                low = id = newMin;
            }

            // do the recursive deletion
            ulong upper = upperAddress(id);
            local[upper].Delete(lowerAddress(id));

            // if the child node became empty by deletion
            if (local[upper].IsEmpty()) {

                // mark the empty child node as unused
                local[upper] = null;
                global.Delete(upper);

                // in case the maximum was deleted
                if (id == high) {

                    // find the new maximum in the children nodes, defaulting to low
                    // if low is the only element left in this data structure
                    ulong? l = global.high;
                    high = (l != null) ? ((ulong)l << lowerBits) | local[l.Value].GetMax() : low;
                }
            }
            // if the maximum was deleted, but there is another element in the child node
            else if (id == high) { high = (upper << lowerBits) | (local[upper].GetMax()); }
        }

        // helper functions for mapping node ids to the corresponding global / local address parts
        private ulong upperAddress(ulong id) => id >> lowerBits;
        private ulong lowerAddress(ulong id) => (((ulong)1 << lowerBits) - 1) & id;
    }
}
