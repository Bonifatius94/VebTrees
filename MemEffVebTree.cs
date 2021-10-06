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
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Numerics;

namespace VebTrees
{
    /// <summary>
    /// An implementation of the van-Emde-Boas tree data structure that can be used
    /// as a priority queue supporting all operations with at most O(log log u) time.
    /// It's a bit more memory efficient than the regular one only allocating max. memory
    /// of O(u) instead of O(u log u) bits.
    /// </summary>
    public class MemEffVebTree : IPriorityQueue
    {
        // info: this class is wrapping up the van-Emde-Boas tree to make its max. memory
        //       requirement more efficient. It's meant to be used by vEB library users.

        /// <summary>
        /// Create a new instance of a van-Emda-Boas tree with the given universe size.
        /// </summary>
        /// <param name="universeBits">The universe size as bits.</param>
        public MemEffVebTree(byte universeBits)
        {
            lowerBits = (byte)Math.Floor(Math.Log2(universeBits));
            upperBits = (byte)(universeBits - lowerBits);

            global = new VebTree(upperBits);
            local = new BinarySearchTree[m];
        }

        private VebTree global;
        private BinarySearchTree[] local;
        private ulong? low = null;
        private ulong? high = null;

        private byte upperBits;
        private byte lowerBits;
        private ulong m => (ulong)1 << upperBits;

        public bool IsEmpty() => low == null;
        public ulong? GetMin() => low;
        public ulong? GetMax() => high;
        public bool Member(ulong id)
            => !global.IsEmpty() && local[upperAddress(id)].Member(lowerAddress(id));
        public ulong? Successor(ulong id) => global.IsEmpty()
            ? null : local[upperAddress(id)].Successor(lowerAddress(id));
        public ulong? Predecessor(ulong id) => global.IsEmpty()
            ? null : local[upperAddress(id)].Predecessor(lowerAddress(id));

        public void Insert(ulong id)
        {
            // make sure the id is not already inserted
            if (Member(id)) { return; }

            ulong upper = upperAddress(id);
            ulong lower = lowerAddress(id);

            // insert the id into global and local
            global.Insert(upper);
            local[upper] = local[upper] ?? new BinarySearchTree(lowerBits);
            local[upper].Insert(lower);

            // update low / high pointers
            if (low == null) { low = high = id; return; }
            low = low != null ? Math.Min(low.Value, id) : id;
            high = high != null ? Math.Max(high.Value, id) : id;
        }

        public void Delete(ulong id)
        {
            // make sure the id is already inserted
            if (!Member(id)) { return; }

            ulong upper = upperAddress(id);
            ulong lower = lowerAddress(id);

            // delete the id from global and local
            global.Delete(upper);
            local[upper].Delete(lower);

            // update low / high pointers
            if (low == high) { low = high = null; return; }
            if (id == low)
            {
                ulong minUpper = global.GetMin().Value;
                low = (minUpper << upperBits) | local[minUpper].GetMin();
            }
            else if (id == high)
            {
                ulong maxUpper = global.GetMax().Value;
                low = (maxUpper << upperBits) | local[maxUpper].GetMax();
            }

            // unallocate binary heap if not in use
            local[upper] = local[upper].IsEmpty() ? null : local[upper];
        }

        // helper functions for mapping node ids to the corresponding global / local address parts
        private ulong upperAddress(ulong id) => id >> lowerBits;
        private ulong lowerAddress(ulong id) => (((ulong)1 << lowerBits) - 1) & id;
    }
}
