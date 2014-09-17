using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework {
    class Document {
        private readonly List<Differential> _differentials;
        private readonly HashSet<int> _activeDifferentialIndices;
 
        public Document() {
            _differentials = new List<Differential>();
            _activeDifferentialIndices = new HashSet<int>();
        }

        public void AppendDifferential(Differential differential, DocumentState originatingState) {
            var contextChangeIndices = new HashSet<int>(_activeDifferentialIndices);
            contextChangeIndices.SymmetricExceptWith(originatingState.ActiveDifferentialIndices);

        }
    }

    public class DocumentState {
        private readonly HashSet<int> _activeDifferentialIndices;

        internal DocumentState(IEnumerable<int> activeDifferentialIndices) {
            _activeDifferentialIndices = new HashSet<int>();
        }

        public HashSet<int> ActiveDifferentialIndices {
            get { return _activeDifferentialIndices; }
        }
    }
}
