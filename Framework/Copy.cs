using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework {
    public class Copy : IStringTransform {
        private readonly uint _sourcePosition;
        private readonly uint _destinationPosition;
        private readonly uint _length;

        public Copy(uint sourcePosition, uint destinationPosition, uint length) {
            _sourcePosition = sourcePosition;
            _destinationPosition = destinationPosition;
            _length = length;
        }

        public uint SourcePosition {
            get { return _sourcePosition; }
        }

        public uint DestinationPosition {
            get { return _destinationPosition; }
        }

        public uint Length {
            get { return _length; }
        }

        public string Apply(string original) {
            return original.Insert((int)_destinationPosition, original.Substring((int)_sourcePosition, (int)_length));
        }
    }
}
