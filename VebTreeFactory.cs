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

namespace VebTrees
{
    /// <summary>
    /// An factory yielding implementations of the van-Emde-Boas tree data structure that can
    /// be used as a priority queue supporting all operations with at most O(log log u) time.
    /// </summary>
    public static class VebTreeFactory
    {
        /// <summary>
        /// Create a new van Emde Boas tree of the given universe size.
        /// </summary>
        /// <param name="universeBits">The vEB tree's universe size.</param>
        /// <returns></returns>
        public static IPriorityQueue CreateTree(byte universeBits)
        {
            // TODO: think of improvements (e.g. using a fully-allocated tree for small universes)
            return new MemEffVebTree(universeBits);
        }

        // TODO: add tree node factory function as well
    }
}
