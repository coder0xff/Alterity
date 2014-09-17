using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework {
    public class Differential {
        private readonly List<IStringTransform> _stringTransforms;

        public Differential(IEnumerable<IStringTransform> stringTransforms) {
            _stringTransforms = new List<IStringTransform>(stringTransforms);
        }

        static IStringTransform[] TuplesToDeletions(IEnumerable<Tuple<uint, uint>> tuples) {
            return tuples.Select(x => new Deletion(x.Item1, x.Item2)).ToArray<IStringTransform>();
        }

        static IStringTransform[] TuplesToInsertions(IEnumerable<Tuple<uint, uint, uint>> tuples, String contents) {
            return
                tuples.Select(x => new Insertion(x.Item1, contents.Substring((int) x.Item3, (int) x.Item2)))
                    .ToArray<IStringTransform>();
        }

        static IStringTransform[] IntroduceAntecedent(Deletion lhs, Deletion rhs) {
            return TuplesToDeletions(StringTransformationOperators.IntroduceAntecedent(Tuple.Create(lhs.Position, lhs.Length),
                Tuple.Create(rhs.Position, rhs.Length)));
        }

        static IStringTransform[] IntroduceAntecedent(Deletion lhs, Insertion rhs) {
            return
                TuplesToDeletions(StringTransformationOperators.IntroduceAntecedent(Tuple.Create(lhs.Position, lhs.Length),
                    Tuple.Create(rhs.Position, rhs.Length, (uint)0)));
        }

        static IStringTransform[] IntroduceAntecedent(Insertion lhs, Deletion rhs) {
            return TuplesToInsertions(StringTransformationOperators.IntroduceAntecedent(Tuple.Create(lhs.Position, lhs.Length, (uint)0),
                Tuple.Create(rhs.Position, rhs.Length)), lhs.Contents);
        }

        static IStringTransform[] IntroduceAntecedent(Insertion lhs, Insertion rhs) {
            return TuplesToInsertions(StringTransformationOperators.IntroduceAntecedent(Tuple.Create(lhs.Position, lhs.Length, (uint)0),
                Tuple.Create(rhs.Position, rhs.Length, (uint)0)), lhs.Contents);
        }

        static IStringTransform[] WithrdawAntecedent(Deletion lhs, Deletion rhs) {
            return
                TuplesToDeletions(StringTransformationOperators.WithdrawAntecedent(Tuple.Create(lhs.Position, lhs.Length),
                    Tuple.Create(rhs.Length, rhs.Position)));

        }

        static IStringTransform[] WithdrawAntecedent(Deletion lhs, Insertion rhs) {
            return TuplesToDeletions(StringTransformationOperators.WithdrawAntecedent(Tuple.Create(lhs.Position, lhs.Length),
                Tuple.Create(rhs.Position, rhs.Length, (uint)0)));
        }

        static IStringTransform[] WithdrawAntecedent(Insertion lhs, Deletion rhs) {
            return TuplesToInsertions(StringTransformationOperators.WithdrawAntecedent(Tuple.Create(lhs.Position, lhs.Length, (uint)0),
                Tuple.Create(rhs.Position, rhs.Length)), lhs.Contents);
        }

        static IStringTransform[] WithdrawAntecedent(Insertion lhs, Insertion rhs) {
            return TuplesToInsertions(StringTransformationOperators.WithdrawAntecedent(Tuple.Create(lhs.Position, lhs.Length, (uint)0),
                Tuple.Create(rhs.Position, rhs.Length, (uint)0)), lhs.Contents);
        }

        static IStringTransform[] IntroduceSubsequent(Deletion lhs, Deletion rhs) {
            return TuplesToDeletions(StringTransformationOperators.IntroduceSubsequent(Tuple.Create(lhs.Position, lhs.Length),
                Tuple.Create(rhs.Position, rhs.Length)));
        }

        static IStringTransform[] IntroduceSubsequent(Deletion lhs, Insertion rhs) {
            return TuplesToDeletions(StringTransformationOperators.IntroduceSubsequent(Tuple.Create(lhs.Position, lhs.Length),
                Tuple.Create(rhs.Position, rhs.Length, (uint)0)));
        }

        static IStringTransform[] IntroduceSubsequent(Insertion lhs, Deletion rhs) {
            return TuplesToInsertions(StringTransformationOperators.IntroduceSubsequent(Tuple.Create(lhs.Position, lhs.Length, (uint)0),
                Tuple.Create(rhs.Position, rhs.Length)), lhs.Contents);
        }

        static IStringTransform[] IntroduceSubsequent(Insertion lhs, Insertion rhs) {
            return TuplesToInsertions(StringTransformationOperators.IntroduceSubsequent(Tuple.Create(lhs.Position, lhs.Length, (uint)0),
                Tuple.Create(rhs.Position, rhs.Length, (uint)0)), lhs.Contents);
        }

        static IStringTransform[] WithdrawSubsequent(Deletion lhs, Deletion rhs) {
            return TuplesToDeletions(StringTransformationOperators.WithdrawSubsequent(Tuple.Create(lhs.Position, lhs.Length),
                Tuple.Create(rhs.Position, rhs.Length)));
        }

        static IStringTransform[] WithdrawSubsequent(Deletion lhs, Insertion rhs) {
            return TuplesToDeletions(StringTransformationOperators.WithdrawSubsequent(Tuple.Create(lhs.Position, lhs.Length),
                Tuple.Create(rhs.Position, rhs.Length, (uint)0)));
        }

        static IStringTransform[] WithdrawSubsequent(Insertion lhs, Deletion rhs) {
            return TuplesToInsertions(StringTransformationOperators.WithdrawSubsequent(Tuple.Create(lhs.Position, lhs.Length, (uint)0),
                Tuple.Create(rhs.Position, rhs.Length)), lhs.Contents);
        }

        static IStringTransform[] WithdrawSubsequent(Insertion lhs, Insertion rhs) {
            return TuplesToInsertions(StringTransformationOperators.WithdrawSubsequent(Tuple.Create(lhs.Position, lhs.Length, (uint)0),
                Tuple.Create(rhs.Position, rhs.Length, (uint)0)), lhs.Contents);
        }

        static IStringTransform[] IntroduceAntecedent(IStringTransform lhs, IStringTransform rhs) {
            bool onIsInsertion = lhs is Insertion;
            bool itemIsInsertion = rhs is Insertion;
            if (onIsInsertion) {
                if (itemIsInsertion) {
                    return IntroduceAntecedent((Insertion) lhs, (Insertion) rhs);
                } else {
                    return IntroduceAntecedent((Insertion) lhs, (Deletion) rhs);
                }
            }
            else {
                if (itemIsInsertion) {
                    return IntroduceAntecedent((Deletion)lhs, (Insertion)rhs);
                } else {
                    return IntroduceAntecedent((Deletion)lhs, (Deletion)rhs);
                }
            }
        }

        static IStringTransform[] WithdrawAntecedent(IStringTransform lhs, IStringTransform rhs) {
            bool onIsInsertion = lhs is Insertion;
            bool itemIsInsertion = rhs is Insertion;
            if (onIsInsertion) {
                if (itemIsInsertion) {
                    return WithdrawAntecedent((Insertion)lhs, (Insertion)rhs);
                } else {
                    return WithdrawAntecedent((Insertion)lhs, (Deletion)rhs);
                }
            } else {
                if (itemIsInsertion) {
                    return WithdrawAntecedent((Deletion)lhs, (Insertion)rhs);
                } else {
                    return WithdrawAntecedent((Deletion)lhs, (Deletion)rhs);
                }
            }
        }

        static IStringTransform[] IntroduceSubsequent(IStringTransform lhs, IStringTransform rhs) {
            bool onIsInsertion = lhs is Insertion;
            bool itemIsInsertion = rhs is Insertion;
            if (onIsInsertion) {
                if (itemIsInsertion) {
                    return IntroduceSubsequent((Insertion)lhs, (Insertion)rhs);
                } else {
                    return IntroduceSubsequent((Insertion)lhs, (Deletion)rhs);
                }
            } else {
                if (itemIsInsertion) {
                    return IntroduceSubsequent((Deletion)lhs, (Insertion)rhs);
                } else {
                    return IntroduceSubsequent((Deletion)lhs, (Deletion)rhs);
                }
            }
        }

        static IStringTransform[] WithdrawSubsequent(IStringTransform lhs, IStringTransform rhs) {
            bool onIsInsertion = lhs is Insertion;
            bool itemIsInsertion = rhs is Insertion;
            if (onIsInsertion) {
                if (itemIsInsertion) {
                    return WithdrawSubsequent((Insertion)lhs, (Insertion)rhs);
                } else {
                    return WithdrawSubsequent((Insertion)lhs, (Deletion)rhs);
                }
            } else {
                if (itemIsInsertion) {
                    return WithdrawSubsequent((Deletion)lhs, (Insertion)rhs);
                } else {
                    return WithdrawSubsequent((Deletion)lhs, (Deletion)rhs);
                }
            }
        }

        static bool TryCoallesce(Deletion antecedent, Deletion subsequent, out IStringTransform[] result) {
            if (subsequent.Position == antecedent.Position) {
                result = new IStringTransform[]
                {new Deletion(antecedent.Position, antecedent.Length + subsequent.Length)};
                return true;
            } else {
                result = new IStringTransform[] {antecedent, subsequent};
                return false;
            }
        }

        static bool TryCoallesce(Deletion antecedent, Insertion subsequent, out IStringTransform[] result) {
            result = new IStringTransform[] { antecedent, subsequent };
            return false;
        }

        static bool TryCoallesce(Insertion antecedent, Deletion subsequent, out IStringTransform[] result) {
            uint lengthReduction = Math.Max(0,
                Math.Min(antecedent.Position + antecedent.Length, subsequent.Position + subsequent.Length) -
                Math.Max(antecedent.Position, subsequent.Position));
            if (lengthReduction > 0) {
                List<IStringTransform> temp = new List<IStringTransform>(2);
                if (lengthReduction < antecedent.Length) {
                    temp.Add(new Insertion(Math.Min(antecedent.Position, subsequent.Position),
                        antecedent.Contents.Remove((int) Math.Max((uint) 0, subsequent.Position - antecedent.Position),
                            (int) lengthReduction)));
                }
                if (lengthReduction < subsequent.Length) {
                    temp.Add(new Deletion(subsequent.Position, subsequent.Length - lengthReduction));
                }
                result = temp.ToArray();
                return true;
            } else {
                result = new IStringTransform[] {antecedent, subsequent};
                return false;
            }
        }

        static bool TryCoallesce(Insertion antecedent, Insertion subsequent, out IStringTransform[] result) {
            if (subsequent.Position >= antecedent.Position &&
                subsequent.Position <= antecedent.Position + antecedent.Length) {
                result = new IStringTransform[] {
                    new Insertion(antecedent.Position,
                        antecedent.Contents.Substring(0, (int) (subsequent.Position - antecedent.Position)) +
                        subsequent.Contents +
                        antecedent.Contents.Substring((int) (subsequent.Position - antecedent.Position)))
                };
                return true;
            }
            else {
                result = new IStringTransform[] { antecedent, subsequent };
                return false;
            }
        }

        static bool TryCoallesce(IStringTransform a, IStringTransform b, out IStringTransform[] result) {
            bool aIsInsertion = a is Insertion;
            bool bIsInsertion = b is Insertion;
            if (aIsInsertion) {
                if (bIsInsertion) {
                    return TryCoallesce(a as Insertion, b as Insertion, out result);
                }
                else {
                    return TryCoallesce(a as Insertion, b as Deletion, out result);
                }
            }
            else {
                if (bIsInsertion) {
                    return TryCoallesce(a as Deletion, b as Insertion, out result);
                } else {
                    return TryCoallesce(a as Deletion, b as Deletion, out result);
                }
            }
        }

        static List<IStringTransform> SwapTransforms(IEnumerable<IStringTransform> source, uint index) {
            var temp = source.ToArray();
            var newAntecedent = WithdrawAntecedent(temp[index + 1], temp[index]);
            var newSubsequent = IntroduceAntecedent(new[] {temp[index]}, newAntecedent);
            var results = new List<IStringTransform>();
            for (uint i = 0; i < index; i++) {
                results.Add(temp[i]);
            }
            results.AddRange(newAntecedent);
            results.AddRange(newSubsequent);
            for (uint i = index + 2; i < temp.Length; i++) {
                results.Add(temp[i]);
            }
            return results;
        }

        static IStringTransform[] Coallesce(IEnumerable<IStringTransform> source) {
            var temp = source.ToList();
            loopStart:
            for (int i = temp.Count - 1; i > 0; i--) {
                var unmodified = temp;
                for (int j = i - 1; j >= 0; j--) {
                    IStringTransform[] coeallesced;
                    if (TryCoallesce(temp[i - 1], temp[i], out coeallesced)) {
                        temp.RemoveRange(j, 2);
                        temp.InsertRange(j, coeallesced);
                        goto loopStart;
                    }
                    else {
                        temp = SwapTransforms(temp, (uint) j);
                    }
                }
                temp = unmodified; //Swapping the element at i back through the list didn't result in any coallescing, so put it back
            }
            return temp.ToArray();
        }

        public static IStringTransform[] IntroduceAntecedent(IStringTransform[] lhs, IStringTransform[] rhs) {
            var temp = new List<IStringTransform>(lhs);
            foreach (var rhsTransform in rhs) {
                var temp2 = new List<IStringTransform>();
                foreach (var lhsTransform in lhs) {
                    temp2.AddRange(IntroduceAntecedent(lhsTransform, rhsTransform));
                }
                temp = temp2;
            }
            return Coallesce(temp);
        }

        public static IStringTransform[] WithdrawAntecedent(IStringTransform[] lhs, IStringTransform[] rhs) {
            var temp = new List<IStringTransform>(lhs);
            foreach (var rhsTransform in rhs.Reverse<IStringTransform>()) {
                var temp2 = new List<IStringTransform>();
                foreach (var lhsTransform in lhs) {
                    temp2.AddRange(IntroduceAntecedent(lhsTransform, rhsTransform));
                }
                temp = temp2;
            }
            return Coallesce(temp);            
        }
        
        public static IStringTransform[] IntroduceSubsequent(IStringTransform[] lhs, IStringTransform[] rhs) {
            var temp = new List<IStringTransform>(lhs);
            foreach (var rhsTransform in rhs) {
                var temp2 = new List<IStringTransform>();
                foreach (var lhsTransform in lhs) {
                    temp2.AddRange(IntroduceSubsequent(lhsTransform, rhsTransform));
                }
                temp = temp2;
            }
            return Coallesce(temp);
        }

        public static IStringTransform[] WithdrawSubsequent(IStringTransform[] lhs, IStringTransform[] rhs) {
            var temp = new List<IStringTransform>(lhs);
            foreach (var rhsTransform in rhs.Reverse<IStringTransform>()) {
                var temp2 = new List<IStringTransform>();
                foreach (var lhsTransform in lhs) {
                    temp2.AddRange(IntroduceSubsequent(lhsTransform, rhsTransform));
                }
                temp = temp2;
            }
            return Coallesce(temp);
        }

        public virtual String Apply(String original) {
            return _stringTransforms.Aggregate(original, (current, stringTransform) => stringTransform.Apply(current));
        }
    }
}
