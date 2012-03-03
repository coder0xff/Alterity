using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Alterity
{
    //assigns a unique sequential index to each distinct grapheme in a text
    public class GraphemeIndexTable
    {
        private int[] indices;
        private Dictionary<Grapheme, int> multiCharacterIndices = new Dictionary<Grapheme,int>();
        private int count;
        public GraphemeIndexTable(Grapheme[] graphemes)
        {
            indices = new int[65536];
            for (int init = 0; init < 65536; init++) indices[init] = -1;
            int indexCounter = 1;
            foreach (Grapheme grapheme in graphemes.Distinct())
            {
                if (grapheme.IsMulticharacter)
                    multiCharacterIndices[grapheme] = indexCounter++;
                else
                    indices[Convert.ToInt16(grapheme[0])] = indexCounter++;
            }
            count = indexCounter;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public int this[Grapheme grapheme]
        {
            get
            {
                if ((Object)grapheme == null) return 0;
                if (grapheme.IsMulticharacter)
                    return multiCharacterIndices[grapheme];
                else
                    return indices[Convert.ToInt16(grapheme[0])];
            }
        }

        public int Count { get { return count; } }
    }
}
