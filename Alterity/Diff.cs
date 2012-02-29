using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity
{
    public static class Diff
    {
        static List<MatchedRange> ComputeRelations(String left, String right, int minimumHunkLength)
        {
            List<MatchedRange> results = new List<MatchedRange>();
            StringIndexer indexer = new StringIndexer(left);
            Grapheme[] rightGraphemes = right.ToGraphemeArray();
            for (int rightIndex = 0; rightIndex < rightGraphemes.Length; )
            {
                MatchedRange nextHunk = indexer.BestMatchSearch(rightGraphemes, rightIndex, minimumHunkLength);
                if (nextHunk != null)
                {
                    rightIndex = nextHunk.Right.UpperBound + 1;
                    results.Add(nextHunk);
                }
                else
                {
                    rightIndex++;
                }
            }
            return results;
        }
    }
}
