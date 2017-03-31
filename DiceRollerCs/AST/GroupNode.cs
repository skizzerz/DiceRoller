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

            for (ushort run = 0; run < numTimes; run++)
            {
                foreach (var ast in Expressions)
                {
					if (reroll)
					{
						rolls += ast.Reroll(conf, root, depth + 1);
					}
					else
					{
						rolls += ast.Evaluate(conf, root, depth + 1);
					}

                    // If the group contains exactly one member, we want to expose all dice rolled in the subtree
                    // e.g. {3d6+4d8} should contain a list of 7 final values, three for the d6s and four for the d8s.
                    // However, if the group contains more than one member, or if the group's sole member is another group,
                    // we want to expose the aggregate instead. e.g. {3d6,4d8} should expose 2 final values, and {{3d6+4d8}}
                    // should expose 1 final value.
                    if (numTimes == 1)
                    {
						_values = ast.Values.ToList();
                    }
                    else
                    {
                        _values.Add(new DieResult()
                        {
                            DieType = DieType.Group,
                            NumSides = 0,
                            Value = ast.Value
                        });
                    }

                    Value += ast.Value;
                }
            }

            return rolls;
        }
    }
}
