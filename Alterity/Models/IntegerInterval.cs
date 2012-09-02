using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class IntegerInterval
    {
        public int Left { get; set; }
        public int Length { get; set; }
        public int Right
        {
            get { return Left + Length - 1; }
            set { Length = value - Left + 1; }
        }

        public IntegerInterval()
        {

        }

        public IntegerInterval(int left, int length)
        {
            Left = left;
            Length = length;
        }
        
        public IntegerInterval[] InsertTransformSelection(IntegerInterval Tranformer)
        {
            if (Tranformer.Left <= Left)
            {
                return new IntegerInterval[] { new IntegerInterval(Left + Tranformer.Length, Length) };
            }
            else if (Tranformer.Left < Left + Length)
            {
                //the insert is in the middle of the selection, so it'll split in two
                return new IntegerInterval[] { new IntegerInterval(Left, Tranformer.Left - Left),
                new IntegerInterval(Tranformer.Left + Tranformer.Length, Length - (Tranformer.Left - Left))};
            }
            else
            {
                return new IntegerInterval[] { this };
            }               
        }

        public IntegerInterval[] DeleteTransformSelection(IntegerInterval asInsertion)
        {
            int leftShiftCount = Math.Max(0, Math.Min(Left - asInsertion.Left, asInsertion.Length));
            int upperBoundMin = Math.Min(Left + Length - 1, asInsertion.Left + asInsertion.Length - 1);
            int lowerBoundMax = Math.Max(Left, asInsertion.Left);
            int lengthReduction = (lowerBoundMax <= upperBoundMin) ? Math.Max(upperBoundMin - lowerBoundMax + 1, 0) : 0;
            int newLength = Length - lengthReduction;
            if (newLength > 0)
            {
                return new IntegerInterval[] { new IntegerInterval(Left - leftShiftCount, newLength) };
            }
            else
            {
                return new IntegerInterval[] { };
            }
        }

        public IntegerInterval[] InsertTransformInsertion(IntegerInterval asInsertion)
        {
            if (asInsertion.Left <= Left)
            {
                return new IntegerInterval[] { new IntegerInterval(Left + asInsertion.Length, Length) };
            }
            else
            {
                return new IntegerInterval[] { this };
            }
        }

        public IntegerInterval[] DeleteTransformInsertion(IntegerInterval asDeletion)
        {
            int leftShift = Math.Max(0, Math.Min(Left - asDeletion.Left, asDeletion.Length));
            return new IntegerInterval[] { new IntegerInterval(Left - leftShift, Length) };
        }

        public IntegerInterval[] InsertTransformInsertionSwappedPrecedence(IntegerInterval asInsertion)
        {
            if (asInsertion.Left < Left)
            {
                return new IntegerInterval[] { new IntegerInterval(Left + asInsertion.Length, Length) };
            }
            else
            {
                return new IntegerInterval[] { this };
            }
        }
        public IntegerInterval Intersection(IntegerInterval other)
        {
            IntegerInterval result = new IntegerInterval();
            if (other.Right >= Left && other.Left <= Right)
            {
                result.Left = Math.Max(other.Left, Left);
                result.Length = Math.Min(other.Right, Right) - result.Left + 1;
            }
            else
            {
                result.Left = 0;
                result.Length = 0;
            }
            return result;
        }
        public IntegerInterval Union(IntegerInterval other)
        {
            IntegerInterval result = new IntegerInterval();
            if (other.Right >= Left && other.Left <= Right)
            {
                result.Left = Math.Min(other.Left, Left);
                result.Length = Math.Max(other.Right, Right) - result.Left + 1;
            }
            else
            {
                result.Left = 0;
                result.Length = 0;
            }
            return result;
        }
        public IntegerInterval[] Subtract(IntegerInterval other)
        {
            if (other.Right >= Left && other.Left <= Right)
            {
                int leftResultLength = Math.Max(other.Left - Left, 0);
                int rightResultLength = Math.Max(Right - other.Right, 0);
                if (leftResultLength + rightResultLength < this.Length)
                {
                    if (leftResultLength > 0)
                    {
                        if (rightResultLength > 0)
                        {
                            return new IntegerInterval[] { new IntegerInterval(Left, leftResultLength), new IntegerInterval(Right - rightResultLength + 1, rightResultLength) };
                        }
                        else
                        {
                            return new IntegerInterval[] { new IntegerInterval(Left, leftResultLength) };
                        }
                    }
                    else
                    {
                        if (rightResultLength > 0)
                        {
                            return new IntegerInterval[] { new IntegerInterval(Right - rightResultLength + 1, rightResultLength) };
                        }
                        else
                        {
                            return new IntegerInterval[] { };
                        }
                    }
                }
            }
            return new IntegerInterval[] { this };
        }
    }
}