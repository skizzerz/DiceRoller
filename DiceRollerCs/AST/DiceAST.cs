using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dice.Exceptions;

namespace Dice.AST
{
    /// <summary>
    /// Represents a node in the dice expression Abstract Syntax Tree
    /// </summary>
    public abstract class DiceAST
    {
        /// <summary>
        /// If true, this node has been evaluated
        /// </summary>
        public bool Evaluated { get; private set; }

        /// <summary>
        /// The final value of this node. This is only valid after
        /// Evaluate() has been called on the node.
        /// </summary>
        public decimal Value { get; protected set; }

        /// <summary>
        /// The underlying dice that were rolled, as well as their values.
        /// This is only valid after Evaluate() has been called on the node.
        /// If no dice were rolled, this will be null.
        /// </summary>
        public abstract IReadOnlyList<DieResult> Values { get; }

        /// <summary>
        /// Evaluates the node, causing it to store its result in Value.
        /// </summary>
        /// <param name="conf">Configuration of the roller</param>
        /// <param name="root">Root of the AST</param>
        /// <param name="depth">Current recursion depth</param>
        /// <returns>Total number of rolls taken to evaluate this subtree</returns>
        internal ulong Evaluate(RollerConfig conf, DiceAST root, uint depth)
        {
            if (depth > conf.MaxRecursionDepth)
            {
                throw new DiceRecursionException(conf.MaxRecursionDepth);
            }

            ulong rolls = EvaluateInternal(conf, root, depth);

            if (rolls > conf.MaxDice)
            {
                throw new TooManyDiceException(conf.MaxDice);
            }

            Evaluated = true;

            return rolls;
        } 

        protected abstract ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth);
    }
}
