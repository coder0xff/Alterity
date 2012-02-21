using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alterity.Indexing;

namespace Alterity
{
    public class Diff
    {
        LinkedList<MatchedRange> ComputeRelations(String left, String right, int minimumHunkLength)
        {
            LinkedList<MatchedRange> results = new LinkedList<MatchedRange>();
            StringIndexer indexer = new StringIndexer(left);
            throw new NotImplementedException();
        }
    }
}
