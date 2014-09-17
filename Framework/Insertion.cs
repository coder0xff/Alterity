using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework {
    public class Insertion : IStringTransform {
        private readonly uint _position;
        private readonly string _contents;

        public Insertion(uint position, string contents) {
            _position = position;
            _contents = contents;
        }

        public uint Position {
            get { return _position; }
        }

        public string Contents {
            get { return _contents; }
        }

        public uint Length {
            get { return (uint)_contents.Length; }
        }

        public string Apply(string original) {
            return original.Insert((int)Position, Contents);
        }
    }
}
