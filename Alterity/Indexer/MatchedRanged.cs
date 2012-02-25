using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity
{
    public struct MatchedRange
    {
        private Range[] left;
        private Range right;
        public MatchedRange(Range[] left, Range right)
        {
            this.left = left;
            this.right = right;
        }
        public IEnumerable<Range> Left { get { return left; } }
        public Range Right { get { return right; } }
        public override bool Equals(object obj)
        {
            if (obj is MatchedRange)
            {
                MatchedRange convertedObj = (MatchedRange)obj;
                return right == convertedObj.right && Enumerable.SequenceEqual(left, convertedObj.left);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            int result = 527 + right.GetHashCode();
            int numberOfEntriesToHash = Math.Min(left.Length, 3); //including up to 3 entries in the computation of the hash should be adequate
            for (int index = 0; index < numberOfEntriesToHash; index++)
            {
                result = result * 31 + left[index].GetHashCode();
            }
            return result;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString")]
        public override string ToString()
        {
            return left.Length.ToString() + " matched locations.";
        }
        public static bool operator ==(MatchedRange left, MatchedRange right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(MatchedRange left, MatchedRange right)
        {
            return !(left == right);
        }
    }
}
