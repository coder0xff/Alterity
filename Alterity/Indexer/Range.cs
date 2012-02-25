using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity.Indexing
{
    public struct Range
    {
        private int lowerBound, upperBound;
        public Range(int lowerBound, int upperBound)
        {
            System.Diagnostics.Debug.Assert(upperBound >= lowerBound, "upperRange must not be less than lowerRange");
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
        }
        public int LowerBound { get { return lowerBound; } }
        public int UpperBound { get { return upperBound; } }
        public int Length { get { return upperBound - lowerBound + 1; } }
        public bool IsEmpty { get { return Length == 0; } }
        public bool Contains(int value) { return lowerBound <= value && upperBound >= value; }
        public override bool Equals(object obj)
        {
            if (obj is Range)
            {
                Range convertedObj = (Range)obj;
                return this.lowerBound == convertedObj.lowerBound && this.upperBound == convertedObj.upperBound;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return (527 + lowerBound) * 31 + upperBound;
        }
        public override string ToString()
        {
            return lowerBound.ToString() + " - " + upperBound.ToString();
        }
        public static bool operator ==(Range left, Range right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Range left, Range right)
        {
            return !(left == right);
        }
    }
}
