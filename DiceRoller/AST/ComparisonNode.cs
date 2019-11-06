using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Represents one or more comparisons. This node is found as part of other nodes,
    /// rather than a standalone node in the AST.
    /// </summary>
    public class ComparisonNode : DiceAST
    {
        private readonly List<Comparison> _comparisons;

        /// <summary>
        /// The list of comparisons to evaluate against; each comparison
        /// is evaluated independently. As long as one comparison succeeds,
        /// the entire node is considered a success.
        /// </summary>
        public IEnumerable<Comparison> Comparisons => _comparisons;

        /// <summary>
        /// ComparisonNodes only occur in special places in the AST
        /// where it does not make sense to obtain their Value or Values.
        /// As such, neither of these return any data.
        /// </summary>
        public override IReadOnlyList<DieResult> Values => new List<DieResult>();

        internal ComparisonNode(CompareOp operation, DiceAST expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
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
            var comps = new List<string>();
            foreach (var comp in _comparisons)
            {

                var c = comp.op switch
                {
                    CompareOp.Equals => "=",
                    CompareOp.GreaterEquals => ">=",
                    CompareOp.GreaterThan => ">",
                    CompareOp.LessEquals => "<=",
                    CompareOp.LessThan => "<",
                    CompareOp.NotEquals => "!=",
                    _ => "<<INVALID COMPAREOP>>",
                };

                c += comp.expr.Value.ToString();
                comps.Add(c);
            }

            return String.Join(", ", comps);
        }

        internal void Add(ComparisonNode comparison)
        {
            _comparisons.AddRange(comparison.Comparisons);
        }

        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            // this doesn't increase depth as there is no actual logic that a ComparisonNode itself performs
            // (in other words, the Expression can be viewed as the ComparisonNode's evaluation)
            long rolls = 0;
            Value = 0;

            foreach (var c in Comparisons)
            {
                rolls += c.expr.Evaluate(data, root, depth);
            }

            return rolls;
        }

        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = 0;

            foreach (var c in Comparisons)
            {
                rolls += c.expr.Reroll(data, root, depth);
            }

            return rolls;
        }

        public bool Compare(decimal theirValue)
        {
            return Comparisons.Any(c =>
            {
                return c.op switch
                {
                    CompareOp.Equals => theirValue == c.expr.Value,
                    CompareOp.GreaterEquals => theirValue >= c.expr.Value,
                    CompareOp.GreaterThan => theirValue > c.expr.Value,
                    CompareOp.LessEquals => theirValue <= c.expr.Value,
                    CompareOp.LessThan => theirValue < c.expr.Value,
                    CompareOp.NotEquals => theirValue != c.expr.Value,
                    _ => throw new InvalidOperationException("Unknown Comparison Operation"),
                };
            });
        }
    }
}
