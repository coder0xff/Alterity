using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity.Indexing
{
    public struct Hunk
    {
        private int lowerBound, upperBound;
        public Hunk(int lowerBound, int upperBound)
        {
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
            if (obj is Hunk)
            {
                Hunk convertedObj = (Hunk)obj;
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
        public static bool operator ==(Hunk left, Hunk right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Hunk left, Hunk right)
        {
            return !(left == right);
        }
    }
}
