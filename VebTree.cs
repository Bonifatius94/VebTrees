using System;

namespace VebTrees
{
    /// <summary>
    /// An interface representing a van-Emde-Boas tree.
    /// </summary>
    public interface IVebTree
    {
        /// <summary>
        /// Checks if the the tree is empty.
        /// </summary>
        /// <returns>a boolean</returns>
        bool IsEmpty();

        /// <summary>
        /// Checks whether the given id is part of the tree.
        /// </summary>
        /// <param name="id">The id to be looked up.</param>
        /// <returns>a boolean</returns>
        bool Member(ulong id);

        /// <summary>
        /// Gets the minimum of the tree.
        /// </summary>
        /// <returns>the minimum's id (or null if the tree is empty)</returns>
        ulong? GetMin();

        /// <summary>
        /// Gets the minimum of the tree.
        /// </summary>
        /// <returns>the minimum's id (or null if the tree is empty)</returns>
        ulong? GetMax();

        /// <summary>
        /// Get the given id's successor from the tree.
        /// </summary>
        /// <param name="id">The id whose successor is to be found.</param>
        /// <returns>the successor's id (or null if there is no successor)</returns>
        ulong? Successor(ulong id);

        /// <summary>
        /// Get the given id's predecessor from the tree.
        /// </summary>
        /// <param name="id">The id whose predecessor is to be found.</param>
        /// <returns>the predecessor's id (or null if there is no predecessor)</returns>
        ulong? Predecessor(ulong id);

        /// <summary>
        /// Insert the given id into the tree.
        /// </summary>
        /// <param name="id">The id to be inserted.</param>
        void Insert(ulong id);

        /// <summary>
        /// Delete the given id from the tree.
        /// </summary>
        /// <param name="id">The id to be deleted.</param>
        void Delete(ulong id);
    }

    /// <summary>
    /// An implementation of the van-Emde-Boas tree data structure that can be used
    /// as a priority queue supporting all operations with at most O(log log u) time.
    /// </summary>
    public class VebTreeNode : IVebTree
    {
        /// <summary>
        /// Create a new instance of a van-Emda-Boas tree with the given universe size.
        /// </summary>
        /// <param name="universeBits">The universe size as bits.</param>
        public VebTreeNode(byte universeBits)
        {
            // assign universe bits
            this.universeBits = universeBits;

            // initialize children nodes (local) and existance bit-vector (global)
            global = new bool[m];
            local = new VebTreeNode[m];

            // init the children nodes on startup (no allocation in base case)
            if (universeBits > 1) {
                for (ulong i = 0; i < m; i++) { local[i] = new VebTreeNode(lowerBits); }
            }

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
        private bool[] global;
        private VebTreeNode[] local;

        public bool IsEmpty() => low == null;
        public ulong? GetMin() => low;
        public ulong? GetMax() => high;

        public bool Member(ulong id) => low != null && high != null 
                                        && id >= low && id <= high && Successor(id - 1) == id;

        public ulong? Successor(ulong id)
        {
            // base case for universe with { 0, 1 }
            // when looking up the 0, check if the 1 is present in the structure as well
            if (universeBits == 1) { return (id == 0 && high.Value == 1) ? (ulong)1 : null; }

            // case when the predecessor is in the neighbour structure -> low is the successor
            if (low != null && id < low) { return low; }

            // case when the id needs to be looked up in a child node

            // subcase 1: id's successor is in the same child as the id itself
            var localChild = local[upperAddress(id)];
            ulong? localMax = localChild?.GetMax();
            if (localMax != null && lowerAddress(id) < localMax) {
                return (upperAddress(id) << lowerBits) | (localChild.Successor(lowerAddress(id)));
            }

            // subcase 2: id's successor is in a successing child node
            for (ulong i = upperAddress(id) + 1; i < m; i++) {
                if (global[i]) { return ((i << lowerBits) | local[i].GetMin()); }
            }

            // subcase 3: if nothing was found until now
            return null;
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

                // mark sure to update global when inserting into an empty child node
                if (local[upper].IsEmpty()) { global[upper] = true; }

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

                // find the new minimum in the children nodes
                // -> delete the new minimum from the child node
                ulong i = ulong.MaxValue;
                while (!global[++i]) { /* nothing to do here ... */ }
                ulong newMin = (i << lowerBits) | (ulong)local[i].GetMin();
                low = id = newMin;
            }

            // do the recursive deletion
            ulong upper = upperAddress(id);
            local[upper].Delete(lowerAddress(id));

            // if the child node became empty by deletion
            if (local[upper].IsEmpty()) {

                // mark the empty child node as unused
                global[upper] = false;

                // in case the maximum was deleted
                if (id == high) {

                    // find the new maximum in the children nodes
                    long i = (long)m;
                    while (--i >= 0 && !global[i]) { /* nothing to do here ... */ }

                    // in case nothing was found -> structure with 1 element left
                    if (i < 0) { high = low; }
                    else { high = ((ulong)i << lowerBits) | (local[i].GetMax()); }
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
