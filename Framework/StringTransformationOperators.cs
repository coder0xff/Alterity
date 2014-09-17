using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using I = System.Tuple<uint, uint, uint>;
using D = System.Tuple<uint, uint>;
namespace Framework {
    internal class StringTransformationOperators {
        public static D[] IntroduceAntecedent(D lhs, D rhs) {
            uint positionReduction = Math.Max(0, Math.Min(rhs.Item2, lhs.Item1 - rhs.Item1)); //The number of elements in rhs that are before lhs
            uint sizeReduction = Math.Max(0, Math.Min(lhs.Item1 + lhs.Item2, rhs.Item1 + rhs.Item2) -
                                 Math.Max(lhs.Item1, lhs.Item2)); //The number of elements that are in the intersection of lhs and rhs
            return new[] {Tuple.Create(lhs.Item1 - positionReduction, lhs.Item2 - sizeReduction)};
        }

        public static D[] IntroduceAntecedent(D lhs, I rhs) {
            if (rhs.Item1 <= lhs.Item1) {
                return new [] {
                    Tuple.Create(lhs.Item1 + rhs.Item2, lhs.Item2)
                };
            }
            if (rhs.Item1 > lhs.Item1 && rhs.Item1 < lhs.Item1 + lhs.Item2) {
                return new[] {
                    Tuple.Create(lhs.Item1, rhs.Item1 - lhs.Item1),
                    Tuple.Create(rhs.Item1 - lhs.Item1 + rhs.Item2, lhs.Item2 - (rhs.Item1 - lhs.Item1))
                };
            }
            return new[] {lhs};
        }

        public static I[] IntroduceAntecedent(I lhs, D rhs) {
            if (rhs.Item1 <= lhs.Item1) {
                return new[] {Tuple.Create(lhs.Item1 - rhs.Item2, lhs.Item2, lhs.Item3)};
            }
            return new[] {lhs};
        }

        public static I[] IntroduceAntecedent(I lhs, I rhs) {
            if (rhs.Item1 <= lhs.Item1) {
                return new[] {Tuple.Create(lhs.Item1 + rhs.Item1, lhs.Item2, lhs.Item3)};
            }
            return new[] {lhs};
        }

        public static D[] WithdrawAntecedent(D lhs, D rhs) {
            return IntroduceAntecedent(lhs, Tuple.Create(rhs.Item1, rhs.Item2, (uint)0));
        }

        public static D[] WithdrawAntecedent(D lhs, I rhs) {
            return IntroduceAntecedent(lhs, Tuple.Create(rhs.Item1, rhs.Item2));
        }

        public static I[] WithdrawAntecedent(I lhs, D rhs) {
            return IntroduceAntecedent(lhs, Tuple.Create(rhs.Item1, rhs.Item2, (uint) 0));
        }

        public static I[] WithdrawAntecedent(I lhs, I rhs) {
            return IntroduceAntecedent(lhs, Tuple.Create(rhs.Item1, rhs.Item2));
        }

        public static D[] IntroduceSubsequent(D lhs, D rhs) {
            return IntroduceAntecedent(lhs, rhs);
        }

        public static D[] IntroduceSubsequent(D lhs, I rhs) {
            return IntroduceAntecedent(lhs, rhs);
        }

        public static I[] IntroduceSubsequent(I lhs, D rhs) {
            if (rhs.Item1 <= lhs.Item1) {
                uint sizeReduction = Math.Max(0, Math.Min(lhs.Item1 + lhs.Item2, rhs.Item1 + rhs.Item2) -
                                     Math.Max(lhs.Item1, lhs.Item2)); //The number of elements that are in the intersection of lhs and rhs
                if (sizeReduction < lhs.Item2) {
                    return new[] {Tuple.Create(rhs.Item1, lhs.Item2 - sizeReduction, lhs.Item3 + sizeReduction)};
                }
                return new I[] {};
            }
            if (rhs.Item1 + rhs.Item2 < lhs.Item1 + lhs.Item2) {
                return new [] {Tuple.Create(lhs.Item1, rhs.Item1 - lhs.Item1, lhs.Item3), Tuple.Create(rhs.Item1, (rhs.Item1 + rhs.Item2) - (lhs.Item1 + lhs.Item2), rhs.Item1 - lhs.Item1 + rhs.Item2)};
            }
            if (rhs.Item1 < lhs.Item1 + lhs.Item2) {
                return new[] {Tuple.Create(lhs.Item1, rhs.Item1 - lhs.Item1, lhs.Item3)};
            }
            return new I[] {};
        }

        public static I[] IntroduceSubsequent(I lhs, I rhs) {
            if (rhs.Item1 <= lhs.Item1) {
                return new[] {Tuple.Create(lhs.Item1 + rhs.Item2, lhs.Item2, lhs.Item3)};
            }
            if (rhs.Item1 > lhs.Item1 && rhs.Item1 < lhs.Item1 + lhs.Item2) {
                return new[] {
                    Tuple.Create(lhs.Item1, rhs.Item1 - lhs.Item1, lhs.Item3),
                    Tuple.Create(rhs.Item1 + rhs.Item2, lhs.Item2 - (rhs.Item1 - lhs.Item1),
                        lhs.Item3 + (rhs.Item1 - lhs.Item1))
                };
            }
            return new[] {lhs};
        }

        public static D[] WithdrawSubsequent(D lhs, D rhs) {
            if (rhs.Item1 > lhs.Item1 && rhs.Item1 + rhs.Item2 < lhs.Item1 + lhs.Item2) {
                throw new InvalidOperationException("The specified operands are not sane.");
            }
            if (rhs.Item1 <= lhs.Item1) {
                return new[] {Tuple.Create(lhs.Item1 + rhs.Item2, lhs.Item2)};
            }
            return new[] {lhs};
        }

        public static D[] WithdrawSubsequent(D lhs, I rhs) {
            if (rhs.Item1 > lhs.Item1 && rhs.Item1 + rhs.Item2 < lhs.Item1 + lhs.Item2) {
                throw new InvalidOperationException("The specified operands are not sane.");
            }
            if (rhs.Item1 <= lhs.Item1) {
                if (lhs.Item1 < rhs.Item2) {
                    throw new InvalidOperationException("The specified operands are not sane.");
                }
                return new[] { Tuple.Create(lhs.Item1 - rhs.Item2, lhs.Item2) };
            }
            return new[] { lhs };            
        }

        public static I[] WithdrawSubsequent(I lhs, D rhs) {
            uint sizeReduction = Math.Max(0, Math.Min(lhs.Item1 + lhs.Item2, rhs.Item1 + rhs.Item2) -
                                 Math.Max(lhs.Item1, lhs.Item2)); //The number of elements that are in the intersection of lhs and rhs
            if (sizeReduction > 0) {
                throw new LostDataException("Cannot perform WithdrawSubsequent(I, D) because IntroduceSubsequent(I, D) deleted information that cannot restored.");
            }
            if (rhs.Item1 <= lhs.Item1) {
                return new[] {Tuple.Create(lhs.Item1 + rhs.Item2, lhs.Item2, lhs.Item3)};
            }
            return new[] {lhs};
        }

        public class LostDataException : Exception {
            public LostDataException(string s) : base(s) {}
        }

        public static I[] WithdrawSubsequent(I lhs, I rhs) {
            uint sizeReduction = Math.Max(0, Math.Min(lhs.Item1 + lhs.Item2, rhs.Item1 + rhs.Item2) -
                                 Math.Max(lhs.Item1, lhs.Item2)); //The number of elements that are in the intersection of lhs and rhs
            if (sizeReduction > 0) {
                throw new InvalidOperationException("The specified operands are not sane.");
            }
            if (rhs.Item1 <= lhs.Item1) {
                return new[] {Tuple.Create(lhs.Item1 - rhs.Item2, lhs.Item2, lhs.Item3)};
            }
            return new[] {lhs};
        }
    }
}
