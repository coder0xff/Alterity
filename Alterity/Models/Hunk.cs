﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Alterity.Models
{
    public abstract class Hunk
    {
        public interface IHunkComparer : IComparer<Hunk>, System.Collections.IComparer { }
        class HunkValueComparer : IHunkComparer
        {
            static DeletionHunk.ValueComparer deletionComparer = new DeletionHunk.ValueComparer();
            static InsertionHunk.ValueComparer insertionComparer = new InsertionHunk.ValueComparer();
            static NoOperationHunk.ValueComparer noOperationComparer = new NoOperationHunk.ValueComparer();

            int intFromHunkType(Type hunkType)
            {
                if (hunkType == typeof(DeletionHunk))
                    return 0;
                else if (hunkType == typeof(InsertionHunk))
                    return 1;
                else // if (hunkType == typeof(Models.NoOperationHunk))
                    return 2;
            }

            public int Compare(Hunk x, Hunk y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                Type xType = x.GetType();
                Type yType = y.GetType();
                int typeDifference = intFromHunkType(xType) - intFromHunkType(yType);
                if (typeDifference != 0) return typeDifference;
                if (xType == typeof(Models.DeletionHunk))
                    return deletionComparer.Compare((DeletionHunk)x, (DeletionHunk)y);
                else if (xType == typeof(Models.InsertionHunk))
                    return insertionComparer.Compare((InsertionHunk)x, (InsertionHunk)y);
                else
                    return noOperationComparer.Compare((NoOperationHunk)x, (NoOperationHunk)y);
            }

            public int Compare(object x, object y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                Hunk castX = (Hunk)x; //fail if it's not a Hunk
                Hunk castY = (Hunk)y; //fail if it's not a Hunk
                return Compare(castX, castY);
            }
        }
        class HunkIdComparer : IHunkComparer
        {
            public int Compare(Hunk x, Hunk y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return x.Id - y.Id;
            }

            public int Compare(object x, object y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                Hunk castX = (Hunk)x; //fail if it's not a Hunk
                Hunk castY = (Hunk)y; //fail if it's not a Hunk
                return Compare(castX, castY);
            }

        }
        static HunkValueComparer valueComparer = new HunkValueComparer();
        public static IHunkComparer ValueComparer { get { return valueComparer; } }
        static HunkIdComparer idComparer = new HunkIdComparer();
        public static IHunkComparer IdComparer { get { return idComparer; } }

        public int Id { get; set; }
        public abstract Hunk[] UndoPrior(Hunk hunk);
        public abstract Hunk[] RedoPrior(Hunk hunk);
        public abstract void Apply(StringBuilder text);
    }
}