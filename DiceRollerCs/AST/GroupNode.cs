using System;
using System.Collections.Generic;
using System.Linq;

namespace Dice.AST
{
    /// <summary>
    /// A group node represents one or more dice expressions that can be rolled multiple times as a set.
    /// Like basic rolls, modifiers can be applied to the group as a whole.
    /// </summary>
    public class GroupNode : DiceAST
    {
        private List<DiceAST> _expressions;
        private List<DieResult> _values;

        /// <summary>
        /// The number of times to evaluate the group. Any decimal part in
        /// NumTimes.Value is truncated.
        /// </summary>
        public DiceAST NumTimes { get; private set; }

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

        internal GroupNode(DiceAST numTimes, List<DiceAST> exprs)
        {
            NumTimes = numTimes ?? throw new ArgumentNullException("numTimes");
            _expressions = exprs ?? throw new ArgumentNullException("exprs");
            _values = new List<DieResult>();

            if (exprs.Count == 0)
            {
                throw new ArgumentException("A dice group must contain at least one expression", "exprs");
            }
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = NumTimes.Evaluate(conf, root, depth + 1);

            rolls += Roll(conf, root, depth, false);

            return rolls;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            return Roll(conf, root, depth, true);
        }

        internal ulong Roll(RollerConfig conf, DiceAST root, uint depth, bool reroll)
        {
            ulong rolls = 0;
            ushort numTimes = (ushort)NumTimes.Value;
            Value = 0;
            bool first = true;

            for (ushort run = 0; run < numTimes; run++)
            {
                foreach (var ast in Expressions)
                {
                    if (reroll || run > 0)
                    {
                        // we want certain variables "fixed" throughout each iteration
                        // for example, in the group 2{(1d6)d8}, we want to roll 1d6 once and then treat it as if the
                        // group was 2{3d8} (or whatever the 1d6 rolled); in other words, we don't re-evaluate the 1d6
                        // every iteration.
                        rolls += ast.Reroll(conf, root, depth + 1);
                    }
                    else
                    {
                        rolls += ast.Evaluate(conf, root, depth + 1);
                    }

                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        _values.Add(new DieResult()
                        {
                            DieType = DieType.Special,
                            NumSides = 0,
                            Value = (decimal)SpecialDie.Add,
                            Flags = 0
                        });
                    }

                    // If the group contains exactly one member, we want to expose all dice rolled in the subtree
                    // e.g. {3d6+4d8} should contain a list of 7 final values, three for the d6s and four for the d8s.
                    // However, if the group contains more than one member, or if the group's sole member is another group,
                    // we want to expose the aggregate instead. e.g. {3d6,4d8} should expose 2 final values, and {{3d6+4d8}}
                    // should expose 1 final value.
                    if (Expressions.Count == 1)
                    {
                        if (ast is GroupNode)
                        {
                            _values.Add(new DieResult()
                            {
                                DieType = DieType.Group,
                                NumSides = 0,
                                Value = ast.Value,
                                // maintain any crit/fumble flags from the underlying dice, combining them together
                                Flags = ast.Values
                                    .Where(d => d.DieType != DieType.Special && !d.Flags.HasFlag(DieFlags.Dropped))
                                    .Select(d => d.Flags & (DieFlags.Critical | DieFlags.Fumble))
                                    .Aggregate((d1, d2) => d1 | d2)
                            });
                        }
                        else
                        {
                            _values.Add(new DieResult()
                            {
                                DieType = DieType.Special,
                                NumSides = 0,
                                Value = (decimal)SpecialDie.OpenParen,
                                Flags = 0
                            });
                            _values.AddRange(ast.Values);
                            _values.Add(new DieResult()
                            {
                                DieType = DieType.Special,
                                NumSides = 0,
                                Value = (decimal)SpecialDie.CloseParen,
                                Flags = 0
                            });
                        }
                    }
                    else
                    {
                        _values.Add(new DieResult()
                        {
                            DieType = DieType.Group,
                            NumSides = 0,
                            Value = ast.Value,
                            // maintain any crit/fumble flags from the underlying dice, combining them together
                            Flags = ast.Values
                                .Where(d => d.DieType != DieType.Special && !d.Flags.HasFlag(DieFlags.Dropped))
                                .Select(d => d.Flags & (DieFlags.Critical | DieFlags.Fumble))
                                .Aggregate((d1, d2) => d1 | d2)
                        });
                    }

                    Value += ast.Value;
                }
            }

            return rolls;
        }
    }
}
