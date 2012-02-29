using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity
{
    public class MatchedRange
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
            MatchedRange convertedObj = obj as MatchedRange;
            if ((Object)convertedObj != null)
            {
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
            if ((Object)left == null) return (Object)right == null;
            return left.Equals(right);
        }
        public static bool operator !=(MatchedRange left, MatchedRange right)
        {
            return !(left == right);
        }
    }
}
