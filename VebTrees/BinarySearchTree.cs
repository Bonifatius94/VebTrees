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
    /// An implementation of a fully-allocated binary search tree using O(u) bits.
    /// </summary>
    internal class BinarySearchTree : IPriorityQueue
    {
        // TODO: binary search tree can only store universe_size - 1 bits
        //       -> think of ways to handle the edge case for storing the 0

        // TODO: considering a universe size of 2^64, this structure would hold only log_2(2^64) = 64
        //       items which could be implemented more efficiently using a ulong bitboard instead of bool[]
        //       -> think of adding a factory to distinguish between bitboard and general approach

        public BinarySearchTree(byte universeBits)
        {
            this.universeBits = universeBits;
            // TODO: handle case for fully-allocated bin. search tree with 64 bit (1 << 64 == 0 != 2^64)
            ulong size = 1ul << universeBits;
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
            // handle special case for the 0 that's not inserted into the binary tree
            if (id == 0) { return getMinChild(rootId, getNodeRank(rootId)); }

            // node has righthand children -> find min child of that subtree
            int rank = getNodeRank(id);
            if (hasRightChildren(id, rank)) { return getMinChild(getRightChild(id, rank), rank-1); }

            // make sure the node is not the root
            // -> do-while can at least explore one parent
            if (id == rootId) { return null; }

            ulong tempId = id;
            
            do
            {
                // get the node's parent
                ulong parentId = getParent(tempId, rank++);

                // node is located at parent's righthand subtree
                // -> move to parent's parent (parent's lefthand subtree is
                //    smaller, righthand was already explored)
                if (getRightChild(parentId, rank) == tempId) { tempId = parentId; continue; }

                // otherwise, node is located at parent's lefthand subtree

                // the node's parent exists -> parent itself is the successor
                if (exists[parentId]) { return parentId; }

                // the parent does not exist, but has righthand children
                // -> get min from parent's righthand subtree
                if (hasRightChildren(parentId, rank)) {
                    return getMinChild(getRightChild(parentId, rank), rank-1); }

                // node is explored now -> explore parent instead
                tempId = parentId;
            }
            // terminate when there are no more parents to explore
            while (tempId != rootId);

            // otherwise, no successor was found
            return null;
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

            // handle special case for the 0 that's not inserted into the binary tree
            if (id == 0) { return; }

            // traverse the binary search tree and set the whole adjacency
            // of the path from root to the id's parent to true, indicating
            // there's at least one node inserted down there
            ulong tempId = rootId;
            int rank = getNodeRank(rootId);
            while (tempId != id)
            {
                hasChildren[tempId] = true;
                tempId = tempId < id
                    ? getRightChild(tempId, rank--)
                    : getLeftChild(tempId, rank--);
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

            // handle special case for the 0 that's not inserted into the binary tree
            if (id == 0) { return; }

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

            // determine the trailing zeros of id
            return BitOperations.TrailingZeroCount(id);
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
            ulong lowerSid = id - stepSize * 2;
            ulong upperSid = id + stepSize * 2;

            // choose the poss. sibling (need to have the same parent)
            return getParent(lowerSid, rank) == getParent(id, rank)
                ? lowerSid : upperSid;
        }

        private ulong getLeftChild(ulong id, int rank) => id - (1ul << (rank - 1));
        private ulong getRightChild(ulong id, int rank) => id + (1ul << (rank - 1));
    }
}
