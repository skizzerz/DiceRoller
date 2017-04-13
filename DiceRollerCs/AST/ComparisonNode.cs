using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    public class ComparisonNode : DiceAST
    {
        private List<Comparison> _comparisons;

        /// <summary>
        /// The list of comparisons to evaluate against; each comparison
        /// is evaluated independently. As long as one comparison succeeds,
        /// the entire node is considered a success.
        /// </summary>
        public IEnumerable<Comparison> Comparisons
        {
            get { return _comparisons; }
        }

        /// <summary>
        /// ComparisonNodes only occur in special places in the AST
        /// where it does not make sense to obtain their Value or Values.
        /// As such, neither of these return any data. The grammar only allows
        /// for numbers or macros to be underneath ComparisonNodes, which
        /// precludes the possibility to have dice rolls.
        /// </summary>
        public override IReadOnlyList<DieResult> Values
        {
            get { return new List<DieResult>(); }
        }

        internal ComparisonNode(CompareOp operation, DiceAST expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            _comparisons = new List<Comparison>()
            {
                new Comparison(operation, expression)
            };
        }

        internal ComparisonNode(IEnumerable<ComparisonNode> comparisons)
        {
            _comparisons = new List<Comparison>();
            foreach (var c in comparisons)
            {
                _comparisons.AddRange(c.Comparisons);
            }

            if (_comparisons.Count == 0)
            {
                throw new ArgumentException("Must have at least one comparison when aggregating ComparisonNodes");
            }
        }

        public override string ToString()
        {
            List<string> comps = new List<string>();
            foreach (var comp in _comparisons)
            {
                string c;

                switch (comp.op)
                {
                    case CompareOp.Equals:
                        c = "=";
                        break;
                    case CompareOp.GreaterEquals:
                        c = ">=";
                        break;
                    case CompareOp.GreaterThan:
                        c = ">";
                        break;
                    case CompareOp.LessEquals:
                        c = "<=";
                        break;
                    case CompareOp.LessThan:
                        c = "<";
                        break;
                    case CompareOp.NotEquals:
                        c = "!=";
                        break;
                    default:
                        c = "<<INVALID COMPAREOP>>";
                        break;
                }

                c += comp.expr.Value.ToString();
                comps.Add(c);
            }

            return String.Join(", ", comps);
        }

        internal void Add(ComparisonNode comparison)
        {
            _comparisons.AddRange(comparison.Comparisons);
        }

        protected override long EvaluateInternal(RollerConfig conf, DiceAST root, int depth)
        {
            // this doesn't increase depth as there is no actual logic that a ComparisonNode itself performs
            // (in other words, the Expression can be viewed as the ComparisonNode's evaluation)
            long rolls = 0;
            Value = 0;

            foreach (var c in Comparisons)
            {
                rolls += c.expr.Evaluate(conf, root, depth);
            }

            return rolls;
        }

        protected override long RerollInternal(RollerConfig conf, DiceAST root, int depth)
        {
            long rolls = 0;

            foreach (var c in Comparisons)
            {
                rolls += c.expr.Reroll(conf, root, depth);
            }

            return rolls;
        }

        public bool Compare(decimal theirValue)
        {
            return Comparisons.Any(c =>
            {
                switch (c.op)
                {
                    case CompareOp.Equals:
                        return theirValue == c.expr.Value;
                    case CompareOp.GreaterEquals:
                        return theirValue >= c.expr.Value;
                    case CompareOp.GreaterThan:
                        return theirValue > c.expr.Value;
                    case CompareOp.LessEquals:
                        return theirValue <= c.expr.Value;
                    case CompareOp.LessThan:
                        return theirValue < c.expr.Value;
                    case CompareOp.NotEquals:
                        return theirValue != c.expr.Value;
                    default:
                        throw new InvalidOperationException("Unknown Comparison Operation");
                }
            });
        }
    }
}
