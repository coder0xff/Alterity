using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity
{
    public class RightRelationCandidateSet
    {
        private Range[] lefts;
        private Range right;
        public RightRelationCandidateSet(Range[] left, Range right)
        {
            this.lefts = left;
            this.right = right;
        }
        public IEnumerable<Range> Lefts { get { return lefts; } }
        public Range Right { get { return right; } }
        public override bool Equals(object obj)
        {
            RightRelationCandidateSet convertedObj = obj as RightRelationCandidateSet;
            if ((Object)convertedObj != null)
            {
                return right == convertedObj.right && Enumerable.SequenceEqual(lefts, convertedObj.lefts);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            int result = 527 + right.GetHashCode();
            int numberOfEntriesToHash = Math.Min(lefts.Length, 3); //including up to 3 entries in the computation of the hash should be adequate
            for (int index = 0; index < numberOfEntriesToHash; index++)
            {
                result = result * 31 + lefts[index].GetHashCode();
            }
            return result;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString")]
        public override string ToString()
        {
            return lefts.Length.ToString() + " matched locations.";
        }
        public static bool operator ==(RightRelationCandidateSet left, RightRelationCandidateSet right)
        {
            if ((Object)left == null) return (Object)right == null;
            return left.Equals(right);
        }
        public static bool operator !=(RightRelationCandidateSet left, RightRelationCandidateSet right)
        {
            return !(left == right);
        }
    }
}
