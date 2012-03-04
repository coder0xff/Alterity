using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity
{
    public class Diff
    {
        public Diff(String left, String right, int minimumRelationLength)
        {
            DiffEngine.Compute(left, right, minimumRelationLength, this);
        }

        private IEnumerable<Relation> relations;

        public IEnumerable<Relation> Relations
        {
            get { return relations; }
            internal set { relations = value; }
        }
        private IEnumerable<Range> deletedLeftRanges;

        public IEnumerable<Range> DeletedLeftRanges
        {
            get { return deletedLeftRanges; }
            set { deletedLeftRanges = value; }
        }
        private IEnumerable<Range> addedRightRanges;

        public IEnumerable<Range> AddedRightRanges
        {
            get { return addedRightRanges; }
            set { addedRightRanges = value; }
        }
    }

    internal static class DiffEngine
    {
        static List<RightRelationCandidateSet> ComputeCandidateRelations(Grapheme[] leftGraphemes, Grapheme[] rightGraphemes, int minimumRelationLength)
        {
            List<RightRelationCandidateSet> results = new List<RightRelationCandidateSet>();
            StringIndexer indexer = new StringIndexer(leftGraphemes);
            for (int rightIndex = 0; rightIndex < rightGraphemes.Length; )
            {
                RightRelationCandidateSet next = indexer.BestMatchSearch(rightGraphemes, rightIndex, minimumRelationLength);
                if (next != null)
                {
                    rightIndex = next.Right.UpperBound + 1;
                    results.Add(next);
                }
                else
                {
                    rightIndex++;
                }
            }
            return results;
        }

        static float FitnessFunction(Relation relation, IndicatorFunction usedLeftRanges)
        {
            if (usedLeftRanges.AnyTrue(relation.Left)) return 0;
            return relation.Length / (float)System.Math.Abs(relation.Left.UpperBound - relation.Right.UpperBound + 1);
        }

        public static void Compute(String left, String right, int minimumRelationLength, Diff result)
        {
            if (minimumRelationLength < 1) throw new ArgumentOutOfRangeException("minimumRelationLength", "minimumRelationLength must be greater than zero.");
            Grapheme[] leftGraphemes = left.ToGraphemeArray();
            Grapheme[] rightGraphemes = right.ToGraphemeArray();
            List<Relation> relations = new List<Relation>();
            IndicatorFunction usedLeftRanges = new IndicatorFunction();
            List<RightRelationCandidateSet> candidateSets = ComputeCandidateRelations(leftGraphemes, rightGraphemes, minimumRelationLength);
            IndicatorFunction matchedRightRanges = new IndicatorFunction();
            while (true)
            {
                int bestCandidateSetIndex = -1;
                Relation bestRelation = null;
                float bestFitness = 0;
                for (int candidateSetIndex = 0; candidateSetIndex < candidateSets.Count; candidateSetIndex++)
                {
                    Range rightCandidate = candidateSets[candidateSetIndex].Right;
                    foreach (Range leftCandidate in candidateSets[candidateSetIndex].Lefts)
                    {
                        Relation candidateRelation = new Relation(leftCandidate, rightCandidate);
                        float candidateFitness = FitnessFunction(candidateRelation, usedLeftRanges);
                        if (candidateFitness > bestFitness)
                        {
                            bestCandidateSetIndex = candidateSetIndex;
                            bestRelation = candidateRelation;
                            bestFitness = candidateFitness;
                        }
                    }
                }
                if (bestFitness > 0)
                {
                    relations.Add(bestRelation);
                    usedLeftRanges.Add(bestRelation.Left);
                    matchedRightRanges.Add(bestRelation.Right);
                    candidateSets.RemoveAt(bestCandidateSetIndex);
                }
                else
                    break;
            }

            //any remaining candidateSets are evaluating to zero fitness. Now we just match them with their closest left range
            foreach (RightRelationCandidateSet candidateSet in candidateSets)
            {
                Range rightRange = candidateSet.Right;
                Relation bestRelation = null;
                int bestSeparation = int.MaxValue;
                foreach (Range leftCandidate in candidateSet.Lefts)
                {
                    int candidateSeparation = Math.Abs(leftCandidate.LowerBound - rightRange.LowerBound);
                    if (candidateSeparation < bestSeparation)
                    {
                        bestRelation = new Relation(leftCandidate, rightRange);
                        bestSeparation = candidateSeparation;
                    }
                }
                relations.Add(bestRelation);
                usedLeftRanges.Add(bestRelation.Left);
                matchedRightRanges.Add(bestRelation.Right);
            }

            result.Relations = relations;
            result.DeletedLeftRanges = usedLeftRanges.Inverse(new Range(0, leftGraphemes.Length - 1)).TrueRanges;
            result.AddedRightRanges = matchedRightRanges.Inverse(new Range(0, rightGraphemes.Length - 1)).TrueRanges;
        }
    }
}
