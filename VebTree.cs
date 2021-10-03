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

namespace VebTrees
{
    /// <summary>
    /// An interface representing a van-Emde-Boas tree (priority queue).
    /// </summary>
    public interface IPriorityQueue
    {
        /// <summary>
        /// Checks if the the priority queue is empty.
        /// </summary>
        /// <returns>a boolean</returns>
        bool IsEmpty();

        /// <summary>
        /// Checks whether the given id is part of the priority queue.
        /// </summary>
        /// <param name="id">The id to be looked up.</param>
        /// <returns>a boolean</returns>
        bool Member(ulong id);

        /// <summary>
        /// Gets the minimum of the priority queue.
        /// </summary>
        /// <returns>the minimum's id (or null if the priority queue is empty)</returns>
        ulong? GetMin();

        /// <summary>
        /// Gets the minimum of the priority queue.
        /// </summary>
        /// <returns>the minimum's id (or null if the priority queue is empty)</returns>
        ulong? GetMax();

        /// <summary>
        /// Get the given id's successor from the priority queue.
        /// </summary>
        /// <param name="id">The id whose successor is to be found.</param>
        /// <returns>the successor's id (or null if there is no successor)</returns>
        ulong? Successor(ulong id);

        /// <summary>
        /// Get the given id's predecessor from the priority queue.
        /// </summary>
        /// <param name="id">The id whose predecessor is to be found.</param>
        /// <returns>the predecessor's id (or null if there is no predecessor)</returns>
        ulong? Predecessor(ulong id);

        /// <summary>
        /// Insert the given id into the priority queue.
        /// </summary>
        /// <param name="id">The id to be inserted.</param>
        void Insert(ulong id);

        /// <summary>
        /// Delete the given id from the priority queue.
        /// </summary>
        /// <param name="id">The id to be deleted.</param>
        void Delete(ulong id);
    }

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
            upperBits = (byte)(universeBits - lowerBits);
            lowerBits = (byte)Math.Ceiling(Math.Log2(universeBits));

            global = new VebTree(upperBits);
            local = new MemEffBinarySearchTree[lowerBits];
        }

        private VebTree global;
        private MemEffBinarySearchTree[] local;
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
            local[upper] = local[upper] ?? new MemEffBinarySearchTree(lowerBits);
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

    /// <summary>
    /// An implementation of a fully-allocated binary search tree using O(u) bits.
    /// </summary>
    internal class MemEffBinarySearchTree : IPriorityQueue
    {
        // TODO: binary search tree can only store universe_size - 1 bits
        //       -> think of ways to handle the edge case for storing the 0

        public MemEffBinarySearchTree(byte universeBits)
        {
            this.universeBits = universeBits;
            ulong size = 1ul << universeBits; // TODO: handle case for fully-allocated tree with 64 bit (1 << 64 == 0 != 2^64)
            hasChildren = new bool[size];
            exists = new bool[size];
        }

        private readonly byte universeBits;
        private ulong rootId => 1ul << (universeBits - 1);

        private ulong? low = null;
        private ulong? high = null;
        private bool[] hasChildren;
        private bool[] exists;

        public bool IsEmpty() => !hasChildren[rootId];
        public bool Member(ulong id) => exists[id];
        public ulong? GetMin() => low;
        public ulong? GetMax() => high;

        public ulong? Successor(ulong id)
        {
            // the binary search tree guarantees that the lefthand children
            // are all smaller and the righthand children are greater than
            // the node which is currently looked at

            // case 1: node has righthand children
            //         -> find min child of that subtree

            // otherwise, node has only lefthand children that are all smaller by def.
            // -> needs to explore other subtrees of the parent node(s)

            // case 2: the parent has lefthand children that are smaller
            //         -> get min from parent's lefthand subtree

            // case 3: the parent exists and has no lefthand children
            //         -> parent is successor

            // case 4: the parent does not exist, has only righthand children
            //         -> get min from parent's righthand subtree

            // case 5: parent has no other children and does not exist itself
            //         -> repeat case 2 for parent's parent, terminate if parent is the root

            // TODO: finish implementation
            throw new NotImplementedException();
        }

        public ulong? Predecessor(ulong id)
        {
            // implement analog to successor
            // search greatest nodes that are smaller than id

            // TODO: finish implementation
            throw new NotImplementedException();
        }

        public void Insert(ulong id)
        {
            // TODO: handle the zero correctly (that's not inserted into the tree)

            // make sure the id is not already inserted
            if (Member(id)) { return; }

            // set the bit for the child to be inserted
            exists[id] = true;

            // update high / low pointers
            low = low != null ? Math.Min(low.Value, id) : id;
            high = high != null ? Math.Max(high.Value, id) : id;

            // traverse the binary search tree and set the whole adjacency
            // of the path from root to the id's parent to true, indicating
            // there's at least one node inserted down there
            ulong tempId = rootId;
            int rank = getNodeRank(rootId);
            while (tempId != id)
            {
                hasChildren[tempId] = true;
                tempId = tempId < id
                    ? getRightChild(tempId, rank)
                    : getLeftChild(tempId, rank);
            }
        }

        public void Delete(ulong id)
        {
            // TODO: handle the zero correctly (that's not inserted into the tree)

            // make sure the id is already inserted
            if (!Member(id)) { return; }

            // delete the id's node from the tree
            exists[id] = false;

            // update high / low pointers
            if (low == high) { low = high = null; return; }
            if (id == low) { low = getMinChild(rootId, getNodeRank(rootId)); }
            else if (id == high) { high = getMaxChild(rootId, getNodeRank(rootId)); }

            // check if node has children
            // -> children adjacency remains unchanged
            if (hasChildren[id]) { return; }

            // update the children adjacency for all parents until
            // a parent has other children as well or it's the root
            int rank = getNodeRank(id);
            ulong parentId = id;
            while (parentId != rootId)
            {
                ulong siblingId = getSibling(parentId, rank);
                parentId = getParent(parentId, rank++);
                if (hasChildren[siblingId] || exists[siblingId]) { break; }
                hasChildren[parentId] = false;
            }
        }

        private ulong? getMinChild(ulong id, int rank)
        {
            ulong min = id;

            // loop until there are no children to be explored
            while (hasChildren[min])
            {
                // visit the child on the left side (if exists)
                if (hasLeftChildren(min, rank)) { min = getLeftChild(min, rank--); }

                // check if the visited node is the min. child -> terminate
                else if (exists[min]) { break; }

                // no lefthand children, parent and its other children are smaller
                // -> explore righthand children (there have to be nodes)
                else { min = getRightChild(min, rank--); }
            }

            // return the greatest child or null if there is none
            return exists[min] ? min : null;
        }

        private ulong? getMaxChild(ulong id, int rank)
        {
            ulong max = id;

            // loop until there are no children to be explored
            while (hasChildren[max])
            {
                // visit the child on the right side (if exists)
                if (hasRightChildren(max, rank)) { max = getRightChild(max, rank--); }

                // check if the visited node is the max. child -> terminate
                else if (exists[max]) { break; }

                // no righthand children, parent and its other children are greater
                // -> explore lefthand children (there have to be nodes)
                else { max = getLeftChild(max, rank--); }
            }

            // return the greatest child or null if there is none
            return exists[max] ? max : null;
        }

        private bool hasLeftChildren(ulong id, int rank)
        {
            if (rank == 0) { return false; }
            ulong leftChild = this.getLeftChild(id, rank);
            return hasChildren[leftChild] || exists[leftChild];
        }

        private bool hasRightChildren(ulong id, int rank)
        {
            if (rank == 0) { return false; }
            ulong rightChild = this.getRightChild(id, rank);
            return hasChildren[rightChild] || exists[rightChild];
        }

        private int getNodeRank(ulong id)
        {
            // this is based on a property of the binary tree nodes where ids
            // consisting of 2^r (for max. r) are positioned at tree rank r,
            // e.g. odd ids are positioned at the lowest rank with r=0, ids
            // divisible by 2^1 are positioned at r=1, ... only root has max. rank
            // -> rank = trailing zeros of id

            // determine the training zeros of id
            int rank = 0;
            while ((id & 1) == 0) { rank++; id >>= 1; }
            return rank;
        }

        private ulong getParent(ulong id, int rank)
        {
            // determine possible parents (+/- stepsize of rank)
            ulong stepSize = 1ul << rank;
            ulong lowerPid = id - stepSize;
            ulong upperPid = id + stepSize;

            // choose the parent with lower rank -> direct parent,
            // i.e. the parent that's of exactly rank r+1
            return ((lowerPid >> (rank + 1)) & 1) == 1 ? lowerPid : upperPid;
        }

        private ulong getSibling(ulong id, int rank)
        {
            // determine possible parents (+/- stepsize of rank)
            ulong stepSize = 1ul << rank;
            ulong lowerPid = id - stepSize * 2;
            ulong upperPid = id + stepSize * 2;

            // choose the poss. sibling with the same rank
            return ((lowerPid >> (rank)) & 1) == 1 ? lowerPid : upperPid;
        }

        private ulong getLeftChild(ulong id, int rank) => id - (ulong)rank;
        private ulong getRightChild(ulong id, int rank) => id + (ulong)rank;
    }

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
