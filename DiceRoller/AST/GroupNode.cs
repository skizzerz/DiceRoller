using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dice.AST
{
    /// <summary>
    /// A group node represents one or more dice expressions that can be rolled multiple times as a set.
    /// Like basic rolls, modifiers can be applied to the group as a whole.
    /// </summary>
    public class GroupNode : DiceAST
    {
        private readonly List<DiceAST> _expressions;
        private readonly List<DieResult> _values;

        /// <summary>
        /// The number of times to evaluate the group. Any decimal part in
        /// NumTimes.Value is truncated. May be null, in which case the group is evaluated once.
        /// </summary>
        public DiceAST? NumTimes { get; private set; }

        /// <summary>
        /// The list of expressions which make up the grouped roll
        /// </summary>
        public IReadOnlyList<DiceAST> Expressions
        {
            get { return _expressions; }
        }

        /// <summary>
        /// The values of each iteration of the grouped roll
        /// </summary>
        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupNode"/> class.
        /// </summary>
        /// <param name="numTimes">Number of times to evaluate this group.</param>
        /// <param name="exprs">Expressions inside of the group.</param>
        internal GroupNode(DiceAST? numTimes, List<DiceAST> exprs)
        {
            if (exprs == null)
            {
                throw new ArgumentNullException(nameof(exprs));
            }
            else if (exprs.Count == 0)
            {
                throw new ArgumentException("A dice group must contain at least one expression", nameof(exprs));
            }

            NumTimes = numTimes;
            _expressions = exprs;
            _values = new List<DieResult>();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (NumTimes != null)
            {
                sb.Append(NumTimes.Value);
            }

            sb.Append('{');
            sb.Append(String.Join(", ", Expressions.Select(o => o.ToString())));
            sb.Append('}');

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            long rolls = NumTimes?.Evaluate(data, root, depth + 1) ?? 0;

            rolls += Roll(data, root, depth);

            return rolls;
        }

        /// <inheritdoc/>
        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            return Roll(data, root, depth);
        }

        /// <summary>
        /// Evaluates this group node.
        /// </summary>
        /// <param name="data">Roll config.</param>
        /// <param name="root">Root of the parsed AST.</param>
        /// <param name="depth">Current evaluation depth.</param>
        /// <returns>Returns the number of dice rolled during group evaluation.</returns>
        internal long Roll(RollData data, DiceAST root, int depth)
        {
            long rolls = 0;
            ushort numTimes = (ushort)(NumTimes?.Value ?? 1);
            bool haveTotal = false;
            bool haveRoll = false;

            Value = 0;
            _values.Clear();

            if (numTimes == 0)
            {
                _values.Add(new DieResult()
                {
                    DieType = DieType.Literal,
                    NumSides = 0,
                    Value = 0,
                    Flags = DieFlags.Extra
                });

                return 0;
            }

            for (ushort run = 0; run < numTimes; run++)
            {
                _values.MaybeAddPlus();
                _values.Add(new DieResult(SpecialDie.OpenParen));

                foreach (var ast in Expressions)
                {
                    // we want certain variables "fixed" throughout each iteration
                    // for example, in the group 2{(1d6)d8}, we want to roll 1d6 once and then treat it as if the
                    // group was 2{3d8} (or whatever the 1d6 rolled); in other words, we don't re-evaluate the 1d6
                    // every iteration. Note that Reroll() calls Evaluate() if the expr wasn't already evaluated.
                    rolls += ast.Reroll(data, root, depth + 1);
                    _values.MaybeAddPlus();

                    // Add a node containing the aggregate result of this expression
                    _values.Add(new DieResult()
                    {
                        DieType = DieType.Group,
                        NumSides = 0,
                        Value = ast.Value,
                        // maintain any crit/fumble flags from the underlying dice, combining them together
                        Flags = ast.Values
                            .Where(d => d.DieType != DieType.Special && !d.Flags.HasFlag(DieFlags.Dropped))
                            .Select(d => d.Flags & (DieFlags.Critical | DieFlags.Fumble))
                            .Aggregate((d1, d2) => d1 | d2),
                        Data = data.InternalContext.AddGroupExpression(ast)
                    });

                    Value += ast.Value;
                    // we make our final ValueType successes if *all* underlying expressions in this group which actually contain rolls are success types
                    // (and if there are actually rolls)
                    if (ast.Values.Any(d => d.DieType.IsRoll()))
                    {
                        haveRoll = true;

                        if (ast.ValueType == ResultType.Total)
                        {
                            haveTotal = true;
                        }
                    }
                }

                _values.Add(new DieResult(SpecialDie.CloseParen));
            }

            if (!haveRoll || haveTotal)
            {
                ValueType = ResultType.Total;
            }
            else
            {
                ValueType = ResultType.Successes;
            }

            return rolls;
        }
    }
}
