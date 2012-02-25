using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity
{
    public class Grapheme : IEquatable<Grapheme>, IComparable<Grapheme>
    {
        private Char[] data;

        public Grapheme(Char[] data) { this.data = data; }

        public Grapheme(String textElement)
        {
            if (textElement == null) throw new ArgumentNullException("textElement");
            if (new System.Globalization.StringInfo(textElement).LengthInTextElements != 1) throw new ArgumentOutOfRangeException("textElement", "The string does not contain exactly one grapheme");
            data = textElement.ToCharArray();
        }

        public bool Equals(Grapheme other)
        {
            if ((Object)other == null) return false;
            if (this.data.Length == other.data.Length)
            {
                for (int index = 0; index < this.data.Length; index++)
                    if (this.data[index] != other.data[index]) return false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int CompareTo(Grapheme other)
        {
            if ((Object)other == null) return 1;
            int compareLength = System.Math.Max(this.data.Length, other.data.Length);
            int index;
            for (index = 0; index < compareLength; index++)
            {
                int charComparison = data[index].CompareTo(other.data[index]);
                if (charComparison != 0) return charComparison;
            }
            return data.Length - other.data.Length;
        }

        public static bool operator <(Grapheme left, Grapheme right)
        {
            if (left == null)
                return right != null;
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Grapheme left, Grapheme right)
        {
            if (left == null) return false;
            return left.CompareTo(right) > 0;
        }

        public override bool Equals(object obj)
        {
            Grapheme cast = obj as Grapheme;
            if ((Object)cast != null)
            {
                return Equals(cast);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int result = 0;
            foreach (char c in data)
            {
                result *= 31;
                result += (int)c;
            }
            return result;
        }

        public override string ToString()
        {
            return new String(data);
        }

        public static bool operator ==(Grapheme left, Grapheme right)
        {
            if ((Object)left == null)
                return (Object)right == null;
            return left.Equals(right);
        }

        public static bool operator !=(Grapheme left, Grapheme right)
        {
            return !(left == right);
        }

        public char this[int index]
        {
            get { return data[index]; }
        }

        public bool IsMulticharacter { get { return data.Length > 1; } }
    }

    public static class GraphemeExtensionMethods
    {
        public static Grapheme[] ToGraphemeArray(this String source)
        {
            List<Grapheme> tempList = new List<Grapheme>();
            System.Globalization.TextElementEnumerator textElementEnumerator = System.Globalization.StringInfo.GetTextElementEnumerator(source);
            while (textElementEnumerator.MoveNext())
                tempList.Add(new Grapheme(textElementEnumerator.GetTextElement()));
            return tempList.ToArray();
        }
    }
}