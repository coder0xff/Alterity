using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity
{
    /// <summary>
    /// Represents a mutable piecewise boolean-valued function, where the domain is the set of integers
    /// Initially, all values evaluate to false. Ranges are added using the add function. Values that fall
    /// inside one or more of the added ranges will then evaluate to true. A red black tree is used to quickly
    /// modify the function and evaluate it.
    /// </summary>
    internal class IndicatorFunction
    {
        public class PositionComparer : IComparer<Range>
        {
            public int Compare(Range x, Range y)
            {
                return x.LowerBound - y.LowerBound;
            }
        }

        private RBTree<Range> hunks;

        public IndicatorFunction()
        {
            hunks = new RBTree<Range>(new PositionComparer());
        }

        public void Add(Range hunk)
        {
            RBTree<Range>.Node tryMergeNode = hunks.GetLessThanOrEqual(hunk);
            if (tryMergeNode == null) tryMergeNode = hunks.GetGreaterThanOrEqual(hunk);
            while (tryMergeNode != null)
            {
                if (tryMergeNode.Value.LowerBound > hunk.UpperBound + 1) break;
                hunk = new Range(Math.Min(tryMergeNode.Value.LowerBound, hunk.LowerBound),
                    Math.Max(tryMergeNode.Value.UpperBound, hunk.UpperBound));
                RBTree<Range>.Node toDelete = tryMergeNode;
                tryMergeNode = RBTree<Range>.NextNode(tryMergeNode);
                hunks.Remove(toDelete.Value);
            }
            hunks.Add(hunk);
        }

        public bool AnyTrue(Range hunk)
        {
            RBTree<Range>.Node testNode = hunks.GetLessThanOrEqual(hunk);
            if (testNode == null) testNode = hunks.GetGreaterThanOrEqual(hunk);
            while (testNode != null)
            {
                if (testNode.Value.LowerBound > hunk.UpperBound) return false;
                if (testNode.Value.UpperBound >= hunk.LowerBound && testNode.Value.LowerBound <= hunk.UpperBound) return true;
            }
            return false;
        }
    }
}
