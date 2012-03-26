using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity
{
    internal abstract class EditOperation
    {
        public Hunk[] Hunks { get; private set; }

        protected EditOperation() {}

        public EditOperation(Hunk[] hunks)
        {
            if (hunks == null) throw new ArgumentNullException("hunks");
            Hunks = (Hunk[])hunks.Clone();
        }

        private T UndoPrior<T>(Hunk hunk) where T : EditOperation, new()
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            List<Hunk> resultingHunks = new List<Hunk>();
            foreach (Hunk unprocessedHunk in Hunks)
                resultingHunks.AddRange(unprocessedHunk.UndoPrior(hunk));
            T result = new T();
            result.Hunks = resultingHunks.ToArray();
            return result;
        }

        internal T UndoPrior<T>(EditOperation operation) where T: EditOperation, new()
        {
            if (operation == null) throw new ArgumentNullException("operation");
            T result = (T)this;
            foreach (Hunk hunk in operation.Hunks.Reverse())
            {
                result = result.UndoPrior<T>(hunk);
            }
            return result;
        }

        internal abstract EditOperation UndoPrior(EditOperation hunk);
    }
}