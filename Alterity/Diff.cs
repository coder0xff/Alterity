using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity
{
    public static class Diff
    {
        static LinkedList<MatchedRange> ComputeRelations(String left, String right, int minimumHunkLength)
        {
            LinkedList<MatchedRange> results = new LinkedList<MatchedRange>();
            StringIndexer indexer = new StringIndexer(left);
            throw new NotImplementedException();
        }
    }
}
