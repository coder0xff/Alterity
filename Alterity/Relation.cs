using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity
{
    public class Relation
    {
        int leftStartIndex;
        int rightStartIndex;
        int length;
        public Relation(Range left, Range right)
        {
            if (left.Length != right.Length) throw new ArgumentException("left and right must have the same length.");
            leftStartIndex = left.LowerBound;
            rightStartIndex = right.LowerBound;
            length = left.Length;
        }
        public Range Left { get { return new Range(leftStartIndex, leftStartIndex + length + 1); } }
        public Range Right { get { return new Range(rightStartIndex, rightStartIndex + length + 1); } }
        public int Length { get { return length; } }
    }
}
