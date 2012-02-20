using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alterity.Indexing;

namespace Alterity
{
    internal class HunkSet
    {
        /// <summary>
        /// Specifies the range that hunks lie in, ie. 0 through universeSize - 1.
        /// </summary>
        private int universeSize;
        private LinkedList<Hunk> hunks;

        /// <summary>
        /// Create a new HunkSet with a specific range (0 through universeSize - 1) that hunks must lie entirely inside of.
        /// </summary>
        /// <param name="universeSize"></param>
        public HunkSet(int universeSize)
        {
            this.universeSize = universeSize;
            hunks = new LinkedList<Hunk>();
        }

        public void AddHunk(Hunk hunk)
        {
            //right now this is just a linear search
            //a tree search that gets you to an approximate node and then linear searches from there may be better
            LinkedListNode<Hunk> currentNode = hunks.First;
            while (currentNode != null && currentNode.Value.UpperBound < hunk.LowerBound)
                currentNode = currentNode.Next;
            if (currentNode == null)
            {
                hunks.AddLast(hunk);
            }
            else
            {
                if (currentNode.Value.LowerBound <= hunk.UpperBound)
                {
                    currentNode.Value = new Hunk(Math.Min(currentNode.Value.LowerBound, hunk.LowerBound), Math.Max(currentNode.Value.UpperBound, hunk.UpperBound));
                }
                else
                {
                    hunks.AddBefore(currentNode, hunk);
                }
            }
        }

        public bool ContainsAny(Hunk hunk)
        {
            //right now this is just a linear search
            //a tree search that gets you to an approximate node and then linear searches from there may be better
            LinkedListNode<Hunk> currentNode = hunks.First;
            while (currentNode != null && currentNode.Value.UpperBound < hunk.LowerBound)
                currentNode = currentNode.Next;
            if (currentNode == null)
            {
                return false;
            }
            else
            {
                if (currentNode.Value.LowerBound <= hunk.UpperBound)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Contains(int value)
        {
            foreach (Hunk hunk in hunks)
            {
                if (hunk.Contains(value)) return true;
            }
            return false;
        }
    }
}
