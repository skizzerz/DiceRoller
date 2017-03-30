using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dice.Exceptions;

namespace Dice.AST
{
    public class RollNode : DiceAST
    {
        private List<decimal> _values;

        /// <summary>
        /// What sort of roll is being made.
        /// </summary>
        public RollType RollType { get; private set; }
        /// <summary>
        /// How many dice are being rolled. Decimals are truncated,
        /// and negative numbers throw a BadDiceException.
        /// </summary>
        public DiceAST NumDice { get; private set; }
        /// <summary>
        /// How many sides does each die have? Decimals are truncated.
        /// Invalid numbers (according to roller config) throw a BadSidesException.
        /// </summary>
        public DiceAST NumSides { get; private set; }
        /// <summary>
        /// The results of each individual die rolled
        /// </summary>
        public IReadOnlyList<decimal> Values
        {
            get { return _values; }
        }

        internal RollNode(RollType rollType, DiceAST numDice, DiceAST numSides)
        {
            if (numDice == null)
            {
                throw new ArgumentNullException("numDice");
            }

            if (numSides == null)
            {
                throw new ArgumentNullException("numSides");
            }

            RollType = rollType;
            NumDice = numDice;
            NumSides = numSides;
        }

        internal override uint Evaluate(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = NumDice.Evaluate(conf, root, depth + 1);
        }
    }
}
