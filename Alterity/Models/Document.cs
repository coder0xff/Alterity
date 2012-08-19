using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Alterity.Models.Async;

namespace Alterity.Models
{
    public class Document
    {
        public int Id { get; set; }
        public virtual ICollection<ChangeSet> ChangeSets { get; set; }
        public User Owner { get; set; }
        public virtual ICollection<User> Administrators { get; set; }
        public virtual ICollection<User> Moderators { get; set; }
        public float? VoteRatioThreshold { get; set; }
        public bool Locked { get; set; }
        public DocumentVisibility Visibility { get; set; }
        public DocumentEditability Editability { get; set; }

        public Document(DocumentVisibility visibility, DocumentEditability editability)
        {
            Visibility = visibility;
            Editability = editability;
            Locked = false;
        }

        public Document() { }

        public static Document GetById(int id)
        {
            return EntityMappingContext.Current.Documents.FirstOrDefault(_ => _.Id == id);
        }

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

        public static String GenerateDocumentText(ICollection<EditOperation> Edits)
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

        public static List<int> GetListOfActiveEditOperationIds(ICollection<EditOperation> editOperations)
        {
            var result = new List<int>(editOperations.Where(_ => _.GetVoteStatus() == EditOperation.ActivationState.Activated).Select(_ => _.Id));
            result.Sort();
            return result;
        }
        /// <summary>
        /// Transforms all the operations in sortedOperations based on the active and inactive
        /// hunks contained in sortedOperations. Essentially, this function reconciles the
        /// state of all edits/hunks so that they could be output into a string
        /// </summary>
        /// <param name="sortedOperations">The operations to process, sorted by Id</param>
        /// <returns></returns>
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

        ChangeSet GetUsersChangeSet(User user)
        {
            ChangeSet result = null;
            foreach (ChangeSet entry in (from changeSet in ChangeSets where changeSet.Owner == user orderby changeSet.Id descending select changeSet).Take(1))
                result = entry;
            if (result == null)
            {
                result = new ChangeSet();
                user.ChangeSets.Add(result);
                this.ChangeSets.Add(result);
            }
            return result;
        }

        IEnumerable<EditOperation> GetCurrentActiveEdits()
        {
            return EntityMappingContext.Current.EditOperations.Where(x => x.Document == this && x.GetVoteStatus() == EditOperation.ActivationState.Activated);
        }

        ChangeSubset GetUsersChangeSubset(User user)
        {
            ChangeSubset result = null;
            foreach (ChangeSubset entry in GetUsersChangeSet(user).ChangeSubsets.OrderByDescending(_ => _.Id).Take(1))
                result = entry;
            if (result == null)
            {
                result = new ChangeSubset(SpringboardState.Create(GetCurrentActiveEdits()));
                GetUsersChangeSet(user).ChangeSubsets.Add(result);
            }
            return result;
        }

        public void CloseAllLowerIndexedEdits(int EndIndex)
        {
            foreach (EditOperation editOperation in EntityMappingContext.Current.EditOperations.Where(x => x.Document == this && x.IsClosed == false && x.Hunks.Min(y => y.StartIndex) > EndIndex))
                editOperation.Close();
        }

        public IEnumerable<EditOperation> GetUsersOpenEditOperations(User user)
        {
            return EntityMappingContext.Current.EditOperations.Where(_ => _.IsClosed == false && _.Document == this && _.User == user);
        }

        public void AppendHunk(Hunk hunk, User user)
        {
            foreach (EditOperation editOperation in GetUsersOpenEditOperations(user).OrderByDescending(_ => _.Hunks.Min(_2 => _2.StartIndex)))
            {
                editOperation.MergeHunk(ref hunk);
                if (hunk == null) break;
            }
            if (hunk is InsertionHunk)
            {
                var operation = new InsertOperation();
                GetUsersChangeSubset(user).EditOperations.Add(operation);
                operation.Hunks.Add(hunk);
            }
            else if (hunk is DeletionHunk)
            {
                var operation = new DeleteOperation();
                GetUsersChangeSubset(user).EditOperations.Add(operation);
                operation.Hunks.Add(hunk);
            }
            else throw new ApplicationException("Cannot create NoOperation operation");
        }
    }
}