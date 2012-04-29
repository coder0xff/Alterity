using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public struct IntegerInterval
    {

        public int Position, Length;

        public IntegerInterval(int startIndex, int length)
        {
            Position = startIndex;
            Length = length;
        }
        
        public IntegerInterval[] InsertTransformSelection(IntegerInterval Tranformer)
        {
            if (Tranformer.Position <= Position)
            {
                return new IntegerInterval[] { new IntegerInterval(Position + Tranformer.Length, Length) };
            }
            else if (Tranformer.Position < Position + Length)
            {
                return new IntegerInterval[] { new IntegerInterval(Position, Tranformer.Position - Position - 1),
                new IntegerInterval(Tranformer.Position + Tranformer.Length, Position + Length + Tranformer.Length - 1)};
            }
            else
            {
                return new IntegerInterval[] { this };
            }               
        }

        public IntegerInterval[] DeleteTransformSelection(IntegerInterval asInsertion)
        {
            int leftShiftCount = Math.Max(0, Math.Min(Position - asInsertion.Position, asInsertion.Length));
            int lengthReduction = Math.Max(0, Math.Min(Math.Min(Position + Length - asInsertion.Position, asInsertion.Position + asInsertion.Length - Position), Length));
            int newLength = Length - lengthReduction;
            if (newLength > 0)
            {
                return new IntegerInterval[] { new IntegerInterval(Position - leftShiftCount, newLength) };
            }
            else
            {
                return new IntegerInterval[] { };
            }
        }

        public IntegerInterval[] InsertTransformInsertion(IntegerInterval asInsertion)
        {
            if (asInsertion.Position <= Position)
            {
                return new IntegerInterval[] { new IntegerInterval(Position + asInsertion.Length, Length) };
            }
            else
            {
                return new IntegerInterval[] { this };
            }
        }

        public IntegerInterval[] DeleteTransformInsertion(IntegerInterval asDeletion)
        {
            int leftShift = Math.Max(0, Math.Min(Position - asDeletion.Position, asDeletion.Length));
            return new IntegerInterval[] { new IntegerInterval(Position - leftShift, Length) };
        }

        public IntegerInterval[] InsertIntoInsertionSwappedPrecedence(IntegerInterval asInsertion)
        {
            if (asInsertion.Position < Position)
            {
                return new IntegerInterval[] { new IntegerInterval(Position + asInsertion.Length, Length) };
            }
            else
            {
                return new IntegerInterval[] { this };
            }
        }
    }
}