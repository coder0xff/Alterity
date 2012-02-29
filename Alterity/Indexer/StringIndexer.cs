using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity
{
    //preprocesses a string so that it can be searched in constant time (linear time related to length of search string)
    internal class StringIndexer
    {
        private GraphemeIndexTable indexTable;
        private Grapheme[] graphemes;

        class GraphemeNode
        {
            private GraphemeIndexTable indexTable;
            public readonly int nodeDepth;
            public GraphemeNode[] children;
            public List<int> documentPositions;

            public GraphemeNode(GraphemeIndexTable indexTable, int nodeDepth)
            {
                this.indexTable = indexTable;
                this.nodeDepth = nodeDepth;
                documentPositions = new List<int>();
                children = new GraphemeNode[indexTable.Count];
            }
            public GraphemeNode this[Grapheme nextGrapheme]
            {
                get
                {
                    int graphemeIndex = indexTable[nextGrapheme];
                    if (graphemeIndex == -1) return null;
                    return children[graphemeIndex];
                }
            }
            public GraphemeNode AddOrRetrieveChildGrapheme(Grapheme nextGrapheme)
            {
                int graphemeIndex = indexTable[nextGrapheme];
                if (children[graphemeIndex] == null) children[graphemeIndex] = new GraphemeNode(indexTable, nodeDepth + 1);
                return children[graphemeIndex];
            }

            public int[] GetDocumentPositions() 
            {
                int[] results = new int[documentPositions.Count];
                for (int index = 0; index < results.Length; index++)
                {
                    results[index] = documentPositions[index] - nodeDepth;
                }
                return results;
            }
        }

        private GraphemeNode root;

        public StringIndexer(String text)
        {
            indexTable = new GraphemeIndexTable(text);
            root = new GraphemeNode(indexTable, -1);
            graphemes = text.ToGraphemeArray();
            LinkedList<GraphemeNode> nodesToBeCheckedForGrowth = new LinkedList<GraphemeNode>();
            for (int index = 0; index < graphemes.Length; index++)
            {
                GraphemeNode childNode = root.AddOrRetrieveChildGrapheme(graphemes[index]);
                childNode.documentPositions.Add(index);
                if (childNode.documentPositions.Count == 2) nodesToBeCheckedForGrowth.AddLast(childNode);
            }
            while(nodesToBeCheckedForGrowth.Count > 0)
            {
                LinkedList<GraphemeNode> nextNodesToBeCheckedForGrowth = new LinkedList<GraphemeNode>();
                foreach (GraphemeNode node in nodesToBeCheckedForGrowth)
                {
                    if (node.documentPositions.Count > 1)
                    {
                        foreach (int nonUniqueGraphemeIndex in node.documentPositions)
                        {
                            int nextGraphemeIndex = nonUniqueGraphemeIndex + 1;
                            bool isEndOfString = nextGraphemeIndex >= graphemes.Length;
                            GraphemeNode childNode = node.AddOrRetrieveChildGrapheme(isEndOfString ? null : graphemes[nextGraphemeIndex]);
                            childNode.documentPositions.Add(nextGraphemeIndex);
                            if (childNode.documentPositions.Count == 2) nextNodesToBeCheckedForGrowth.AddLast(childNode);
                        }
                    }
                }
                nodesToBeCheckedForGrowth = nextNodesToBeCheckedForGrowth;
            }
        }

        /// <summary>
        /// Searched the indexed string for all exact instances of a substring, starting at startingIndex, of searchString.
        /// </summary>
        /// <param name="searchString">The string to search for</param>
        /// <param name="startingIndex">The index of the first grapheme in searchString to search for</param>
        /// <returns></returns>
        public int[] ExactSearch(String searchString, int startingIndex = 0)
        {
            Grapheme[] searchGraphemes = searchString.ToGraphemeArray();
            int searchGraphemeCount = searchGraphemes.Length;
            GraphemeNode currentNode = root;
            for (int index = startingIndex; index < searchGraphemeCount; index++)
            {
                GraphemeNode nextNode = currentNode[searchGraphemes[index]];
                if (nextNode == null)
                {
                    if (currentNode.documentPositions.Count != 1) return new int[0];
                    int remainingSearchLength = searchGraphemeCount - index;
                    int remainingDocumentPosition = currentNode.documentPositions[0] + 1;
                    int remainingDocumentLength = graphemes.Length - remainingDocumentPosition;
                    if (remainingSearchLength > remainingDocumentLength) return new int[0];
                    for (; index < searchGraphemeCount; index++)
                    {
                        if (graphemes[remainingDocumentPosition++] != searchGraphemes[index]) return new int[0];
                    }
                    return new int[] {currentNode.documentPositions[0] - currentNode.nodeDepth};
                }
                else
                {
                    currentNode = nextNode;
                }
            }
            return currentNode.GetDocumentPositions();
        }

        static private MatchedRange CreateHunkFromResults(GraphemeNode terminalNode, int startingIndex, int terminalIndex, int minimumLength)
        {
            Range matchStringHunk = new Range(startingIndex, terminalIndex);
            int hunkLength = terminalIndex - startingIndex + 1;
            if (hunkLength < minimumLength)
            {
                return null;
            }
            else
            {
                Range[] matches = new Range[terminalNode.documentPositions.Count];
                int index = 0;
                foreach (int terminalNodeUpperBound in terminalNode.documentPositions)
                {
                    int hunkLowerBound = terminalNodeUpperBound - terminalNode.nodeDepth;
                    int hunkUpperBound = hunkLowerBound + hunkLength - 1;
                    matches[index++] = new Range(hunkLowerBound, hunkUpperBound);
                }
                return new MatchedRange(matches, matchStringHunk);
            }
        }

        /// <summary>
        /// Find the location(s) in the indexed string that match as many character from matchString as possible,
        /// with the first compared grapheme in matchString at startingIndex. If the length of the matching
        /// graphemes is less than minimumLength, then a MatchedHunk with no left hunks is returned
        /// </summary>
        internal MatchedRange BestMatchSearch(Grapheme[] searchGraphemes, int startingIndex, int minimumLength)
        {
            int searchGraphemeCount = searchGraphemes.Length;
            GraphemeNode currentNode = root;
            int index;
            for (index = startingIndex; index < searchGraphemeCount; index++)
            {
                GraphemeNode nextNode = currentNode[searchGraphemes[index]];
                if (nextNode == null)
                {
                    if (currentNode.documentPositions.Count != 1) //the search terminates because the next grapheme in searchGraphemes results in a string with no matches
                    {
                        if (index == startingIndex)
                        {
                            startingIndex++;
                            currentNode = root;
                            continue;
                        }
                        return CreateHunkFromResults(currentNode, startingIndex, index - 1, minimumLength);
                    }
                    else //we have found a node with a unique position, so we don't terminate the search yet. Now, we go to grapheme-by-grapheme comparison
                    {
                        int remainingDocumentPosition = currentNode.documentPositions[0] + 1;
                        for (; index < searchGraphemeCount && remainingDocumentPosition < graphemes.Length; index++)
                        {
                            if (graphemes[remainingDocumentPosition++] != searchGraphemes[index])
                                break; //found a differing grapheme, so use the return below
                        }
                        return CreateHunkFromResults(currentNode, startingIndex, index - 1, minimumLength); //found the end of the matching characters, which includes the possibility of reaching the end of matchString, or the end of the indexed string
                    }
                }
                else
                {
                    currentNode = nextNode;
                }
            }
            return CreateHunkFromResults(currentNode, startingIndex, index - 1, minimumLength); //reached the end of matchString while still using the index graph
        }
    }
}
