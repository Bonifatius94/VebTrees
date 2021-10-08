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
}
