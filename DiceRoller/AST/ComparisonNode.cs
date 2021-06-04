using System;
using System.Collections.Generic;
using System.Globalization;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonNode"/> class
        /// with a single comparison.
        /// </summary>
        /// <param name="operation">Type of comparison being made.</param>
        /// <param name="expression">Expression to compare with.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonNode"/> class
        /// by combining existing ComparisonNodes into a single one.
        /// </summary>
        /// <param name="comparisons">Comparisons to check against.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonNode"/> class
        /// with a list of existing Comparisons.
        /// </summary>
        /// <param name="comparisons">Comparisons to check against.</param>
        internal ComparisonNode(IEnumerable<Comparison> comparisons)
        {
            _comparisons = new List<Comparison>(comparisons);

            if (_comparisons.Count == 0)
            {
                throw new ArgumentException("Must have at least one comparison when aggregating ComparisonNodes");
            }
        }

        /// <inheritdoc/>
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

                c += comp.expr.Value.ToString(CultureInfo.InvariantCulture);
                comps.Add(c);
            }

            return String.Join(", ", comps);
        }

        /// <summary>
        /// Add a comparison to this node. All comparisons are tested
        /// independently, and the node is considered successful if at least
        /// one comparison succeeds.
        /// </summary>
        /// <param name="comparison">Comparison to add.</param>
        internal void Add(ComparisonNode comparison)
        {
            _comparisons.AddRange(comparison.Comparisons);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = 0;

            foreach (var c in Comparisons)
            {
                rolls += c.expr.Reroll(data, root, depth);
            }

            return rolls;
        }

        /// <summary>
        /// Determines whether any of the comparisons added to this node
        /// match the given value.
        /// </summary>
        /// <param name="theirValue">Value to compare against.</param>
        /// <returns>Returns true if at least one comparison matches the value, false otherwise.</returns>
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
