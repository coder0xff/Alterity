using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework {
    public class Move : IStringTransform {
        private readonly uint _sourcePosition;
        private readonly uint _destinationPosition;
        private readonly uint _length;
        public string Apply(string original) {
            return original.Remove((int)_sourcePosition, (int)_length).Insert((int)_destinationPosition, original.Substring((int)_sourcePosition, (int)_length));
        }
    }
}
