using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alterity.Indexing;

namespace Alterity
{
    public class Diff
    {
        LinkedList<MatchedHunk> ComputeRelations(String left, String right, int minimumHunkLength)
        {
            LinkedList<MatchedHunk> results = new LinkedList<MatchedHunk>();
            StringIndexer indexer = new StringIndexer(left);

        }
    }
}
