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
            local = new IPriorityQueue[m];

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
        private IPriorityQueue global;
        private IPriorityQueue[] local;

        public bool IsEmpty() => low == null;
        public ulong? GetMin() => low;
        public ulong? GetMax() => high;

        public bool Member(ulong id)
            => low != null && (id == low || id == high
                || (id > low && id < high 
                    && local[upperAddress(id)]?.Member(lowerAddress(id)) == true));

        public ulong? Successor(ulong id)
        {
            // base case for universe with { 0, 1 }
            // when looking up the 0, check if the 1 is present in the structure as well
            if (universeBits == 1) { return (id == 0 && high == 1) ? (ulong)1 : null; }

            // case when the predecessor is in the neighbour structure -> low is the successor
            if (low != null && id < low) { return low; }

            // case when the id needs to be looked up in a child node

            ulong upper = upperAddress(id);
            ulong lower = lowerAddress(id);

            // subcase 1: id's successor is in the same child as the id itself
            var localChild = local[upper];
            ulong? localMax = localChild?.GetMax();
            if (localMax != null && lower < localMax) {
                return (upper << lowerBits) | (localChild.Successor(lower));
            }

            // subcase 2: id's successor is in a successing child node,
            //            defaulting to null if there's no successor
            ulong? succ = global?.Successor(upperAddress(id));
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
                local[upper] = local[upper] ?? createNode(lowerBits);

                // mark sure to update global when inserting into an empty child node
                if (local[upper].IsEmpty()) {
                    global = global ?? createNode(upperBits);
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
            if (low == high) { low = high = null; return; }

            // base case with universe { 0, 1 } -> flip the bit
            if (universeBits == 1) { low = high = 1 - id; return; }

            // case when deleting the minimum
            if (id == low) {

                if (global.GetMin() == null) { throw new InvalidOperationException(
                    "global.low is null, this should never happen..."); }

                // find the new minimum in the children nodes
                // -> delete the new minimum from the child node
                ulong i = global.GetMin().Value;
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
                    ulong? l = global.GetMax();
                    high = (l != null) ? ((ulong)l << lowerBits) | local[l.Value].GetMax() : low;
                }
            }
            // if the maximum was deleted, but there is another element in the child node
            else if (id == high) { high = (upper << lowerBits) | (local[upper].GetMax()); }
        }

        // helper functions for mapping node ids to the corresponding global / local address parts
        private ulong upperAddress(ulong id) => id >> lowerBits;
        private ulong lowerAddress(ulong id) => (((ulong)1 << lowerBits) - 1) & id;

        private IPriorityQueue createNode(byte universeBits)
        {
            return universeBits <= 6
                ? new BitwiseVebTreeLeaf(universeBits)
                : new VebTreeNode(universeBits);
        }
    }

    internal struct BitwiseVebTreeLeaf : IPriorityQueue
    {
        public BitwiseVebTreeLeaf(byte universeBits)
        {
            // make sure this struct only manages max. 64 items
            // to fit everything into a single 64-bit integer
            if (universeBits > 6) { throw new ArgumentException(
                "Universe is too big, cannot be greater than 64!"); }

            this.universeBits = universeBits;
            bitboard = 0;
        }

        private byte universeBits;
        private ulong bitboard;

        public bool IsEmpty() => bitboard == 0;

        public bool Member(ulong id) => (bitboard & (1ul << (byte)id)) > 0;

        public ulong? GetMin() => IsEmpty() ? null
            : (ulong)BitOperations.TrailingZeroCount(bitboard);

        public ulong? GetMax() => IsEmpty() ? null
            : (ulong)BitOperations.Log2(bitboard);

        public ulong? Successor(ulong id)
        {
            // extract the minimal of all higher bits
            ulong succBits = bitboard & (0xFFFFFFFFFFFFFFFFul << ((byte)id + 1));
            ulong minSucc = (ulong)BitOperations.TrailingZeroCount(succBits);
            return (minSucc == 0 || succBits == 0) ? null : minSucc;
        }

        public ulong? Predecessor(ulong id)
        {
            // extract the highest of all lower bits
            ulong predBits = bitboard & ((1ul << (byte)id) - 1);
            ulong maxPred = (ulong)BitOperations.Log2(predBits);
            return (maxPred == 0 && (bitboard & 1) == 0) ? null : maxPred;
        }

        public void Insert(ulong id)
        {
            // make sure the bit of id gets set (or stays set)
            bitboard |= 1ul << (byte)id;
        }

        public void Delete(ulong id)
        {
            // make sure the bit of id gets wiped (or stays wiped)
            bitboard &= ~(1ul << (byte)id);
        }
    }
}
