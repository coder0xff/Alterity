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

        private RBTree<Range> trueRanges;

        public IEnumerable<Range> TrueRanges { get { return trueRanges; } }

        public IndicatorFunction()
        {
            trueRanges = new RBTree<Range>(new PositionComparer());
        }

        public void Add(Range trueRange)
        {
            RBTree<Range>.Node tryMergeNode = trueRanges.GetLessThanOrEqual(trueRange);
            if (tryMergeNode == null) tryMergeNode = trueRanges.GetGreaterThanOrEqual(trueRange);
            while (tryMergeNode != null)
            {
                if (tryMergeNode.Value.LowerBound > trueRange.UpperBound + 1) break;
                trueRange = new Range(Math.Min(tryMergeNode.Value.LowerBound, trueRange.LowerBound),
                    Math.Max(tryMergeNode.Value.UpperBound, trueRange.UpperBound));
                RBTree<Range>.Node toDelete = tryMergeNode;
                tryMergeNode = RBTree<Range>.NextNode(tryMergeNode);
                trueRanges.Remove(toDelete.Value);
            }
            trueRanges.Add(trueRange);
        }

        public bool AnyTrue(Range testRange)
        {
            RBTree<Range>.Node testNode = trueRanges.GetLessThanOrEqual(testRange);
            if (testNode == null) testNode = trueRanges.GetGreaterThanOrEqual(testRange);
            while (testNode != null)
            {
                if (testNode.Value.LowerBound > testRange.UpperBound) return false;
                if (testNode.Value.UpperBound >= testRange.LowerBound && testNode.Value.LowerBound <= testRange.UpperBound) return true;
            }
            return false;
        }

        public IndicatorFunction Inverse(Range universe)
        {
            IndicatorFunction result = new IndicatorFunction();
            Range first = trueRanges.First();
            Range last = trueRanges.Last();
            if (universe.LowerBound > first.LowerBound || universe.UpperBound < last.UpperBound) throw new ArgumentOutOfRangeException("universe", "The universe does not contain the domain of the indicator function.");
            int currentLowerBound = universe.LowerBound;
            foreach (Range range in trueRanges)
            {
                if (currentLowerBound >= range.LowerBound) continue;
                result.Add(new Range(currentLowerBound, range.LowerBound - 1));
                currentLowerBound = range.UpperBound + 1;
            }
            if (currentLowerBound <= universe.UpperBound)
                result.Add(new Range(currentLowerBound, universe.UpperBound));
            return result;
        }
    }
}
