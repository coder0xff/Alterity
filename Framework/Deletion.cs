using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework {
    public class Deletion : IStringTransform {
        private readonly uint _position;
        private readonly uint _length;

        public Deletion(uint position, uint length) {
            _position = position;
            _length = length;
        }

        public uint Position {
            get { return _position; }
        }

        public uint Length {
            get { return _length; }
        }

        public string Apply(string original) {
            return original.Remove((int)_position, (int)_length);
        }
    }
}
