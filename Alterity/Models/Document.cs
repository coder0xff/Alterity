using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Alterity.Models
{
    public class Document
    {
        public int Id { get; set; }
        public virtual ICollection<ChangeSet> ChangeSets { get; set; }     
        public UserData Owner { get; set; }
        public virtual ICollection<UserData> Administrators { get; set; }
        public virtual ICollection<UserData> Moderators { get; set; }
        public float? VoteRatioThreshold { get; set; }
        public bool Locked { get; set; }
        public ICollection<EditOperation> GetEditOperations()
        {
            List<EditOperation> results = new List<EditOperation>();
            foreach (ChangeSet changeSet in ChangeSets)
            {
                foreach (ChangeSubset changeSubset in changeSet.ChangeSubsets)
                {
                    foreach (EditOperation editOperation in changeSubset.EditOperations)
                    {
                        results.Add(editOperation);
                    }
                }
            }
            return results;
        }

        public static String Generate(ICollection<EditOperation> Edits)
        {
            StringBuilder result = new StringBuilder();
            foreach (EditOperation edit in Edits)
                edit.Apply(result);
            return result.ToString();
        }

        struct UndoRedoEntry
        {
            public UndoRedoEntry(int operationId, bool isRedo) { this.OperationId = operationId; this.IsRedo = isRedo; }
            public int OperationId;
            public bool IsRedo;
        }

//         static void ComputeStateDifferences(int operationId, IList<int> sortedSpringBoard, IList<int> sortedTargetState, IList<UndoRedoEntry> undosRedos)
//         {
//             int i = 0, j = 0;
//             while (i < sortedSpringBoard.Count && j < sortedTargetState.Count)
//             {
//                 if (sortedTargetState[j] >= operationId) break;
//                 if (sortedSpringBoard[i] == sortedTargetState[j])
//                 {
//                     i++; j++;
//                 }
//                 else if (sortedSpringBoard[i] < sortedTargetState[j])
//                 {
//                     undosRedos.Add(new UndoRedoEntry(sortedSpringBoard[i++], false));
//                 }
//                 else
//                 {
//                     undosRedos.Add(new UndoRedoEntry(sortedTargetState[j++], true));
//                 }
//             }
//             for ( ; i < sortedSpringBoard.Count; i++)
//                 undosRedos.Add(new UndoRedoEntry(sortedSpringBoard[i++], false));
//             for ( ; j < sortedTargetState.Count && sortedTargetState[j] < operationId; j++)
//                 undosRedos.Add(new UndoRedoEntry(sortedTargetState[j++], true));
//         }

        static void ComputeStateDifferences(int operationId, IEnumerable<int> sortedSpringBoard, IEnumerable<int> sortedTargetState, IList<UndoRedoEntry> undosRedos)
        {
            IEnumerator<int> i = sortedSpringBoard.GetEnumerator(), j = sortedTargetState.GetEnumerator();
            i.Reset(); j.Reset();
            do
            {
                if (j.Current >= operationId) break;
                if (i.Current == j.Current)
                {
                    if (!i.MoveNext() || !j.MoveNext()) break;
                }
                else if (i.Current < j.Current)
                {
                    undosRedos.Add(new UndoRedoEntry(i.Current, false));
                    if (!i.MoveNext())
                    {
                        do 
                        {
                            if (j.Current < operationId)
                                undosRedos.Add(new UndoRedoEntry(j.Current, true));
                            else
                                break;
                        } while (j.MoveNext());
                        break;
                    }
                }
                else
                {
                    undosRedos.Add(new UndoRedoEntry(j.Current, true));
                    if (!j.MoveNext())
                    {
                        do 
                        {
                            undosRedos.Add(new UndoRedoEntry(i.Current, false));
                        } while (i.MoveNext());
                        break;
                    }
                }
            } while (true);
        }

        public static EditOperation TransformForTargetState(EditOperation transformee, List<int> sortedTargetState, Dictionary<int, EditOperation> transformedOperations, Dictionary<int, EditOperation> allOperations)
        {
            EditOperation result;
            if (!transformedOperations.TryGetValue(transformee.Id, out result))
            {
                SpringboardState springBoardState = transformee.ChangeSubset.SpringboardState;
                List<int> sortedSpringBoard = new List<int>(springBoardState.Entries.Select(x => x.EditOperationId));
                sortedSpringBoard.Sort();
                List<UndoRedoEntry> undosRedos = new List<UndoRedoEntry>();
                ComputeStateDifferences(transformee.Id, sortedSpringBoard, sortedTargetState, undosRedos);
                result = transformee;
                foreach (UndoRedoEntry undoRedoEntry in undosRedos)
                {
                    int transformerId = undoRedoEntry.OperationId;
                    EditOperation transformer = TransformForTargetState(allOperations[transformerId], sortedTargetState, transformedOperations, allOperations);
                    if (undoRedoEntry.IsRedo)
                        result = result.RedoPrior(transformer);
                    else
                        result = result.UndoPrior(transformer);
                }
                transformedOperations[transformee.Id] = result;
            }
            return result;
        }

        public static EditOperation TransformForTargetState(EditOperation transformee, List<int> sortedTargetState, Dictionary<int, EditOperation> transformedOperations, ICollection<EditOperation> allOperations)
        {
            EditOperation result;
            if (!transformedOperations.TryGetValue(transformee.Id, out result))
            {
                SpringboardState springBoardState = transformee.ChangeSubset.SpringboardState;
                List<int> sortedSpringBoard = new List<int>(springBoardState.Entries.Select(x => x.EditOperationId));
                sortedSpringBoard.Sort();
                List<UndoRedoEntry> undosRedos = new List<UndoRedoEntry>();
                ComputeStateDifferences(transformee.Id, sortedSpringBoard, sortedTargetState, undosRedos);
                result = transformee;
                foreach (UndoRedoEntry undoRedoEntry in undosRedos)
                {
                    int transformerId = undoRedoEntry.OperationId;
                    EditOperation transformer = TransformForTargetState(allOperations.First(x => (x.Id == transformerId)), sortedTargetState, transformedOperations, allOperations);
                    if (undoRedoEntry.IsRedo)
                        result = result.RedoPrior(transformer);
                    else
                        result = result.UndoPrior(transformer);
                }
                transformedOperations[transformee.Id] = result;
            }
            return result;
        }

        public static ICollection<EditOperation> ProcessState(ICollection<EditOperation> sortedOperations)
        {
            int reserveCount = sortedOperations.Count;
            List<EditOperation> operationsToProcess = new List<EditOperation>(reserveCount);
            List<int> sortedTargetState = new List<int>(reserveCount);
            Dictionary<int, EditOperation> allOperations = new Dictionary<int, EditOperation>(reserveCount);
            Dictionary<int, EditOperation> transformedOperations = new Dictionary<int, EditOperation>(reserveCount);
            foreach (EditOperation operation in sortedOperations)
            {
                if (operation.GetVoteStatus() == EditOperation.ActivationState.Activated)
                {
                    operationsToProcess.Add(operation);
                    sortedTargetState.Add(operation.Id);
                }
                allOperations.Add(operation.Id, operation);
            }
            return new List<EditOperation>(operationsToProcess.Select(x => TransformForTargetState(x, sortedTargetState, transformedOperations, allOperations)));
        }
    }
}