using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity
{
    public abstract class EditOperation
    {
        public int Id { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hunk> Hunks { get; set; }


        protected EditOperation() {}

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

        //The return value is an instance of the implementing class
        internal abstract EditOperation UndoPrior(EditOperation hunk);
    }
}