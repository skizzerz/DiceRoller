using System;
using System.Text;
using System.Collections.Generic;


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
        /// If no dice were rolled, this will be an empty list.
        /// </summary>
        public abstract IReadOnlyList<DieResult> Values { get; }

        /// <summary>
        /// Retrieves a normalized representation of the dice expression.
        /// This may differ from the exact string that was typed in by the user.
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

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
                throw new DiceException(DiceErrorCode.RecursionDepthExceeded, conf.MaxRecursionDepth);
            }

            ulong rolls = EvaluateInternal(conf, root, depth);

            if (rolls > conf.MaxDice)
            {
                throw new DiceException(DiceErrorCode.TooManyDice, conf.MaxDice);
            }

            Evaluated = true;

            return rolls;
        }

        /// <summary>
        /// Re-do the roll without re-evaluating the entire subtree again
        /// </summary>
        /// <param name="conf">Roller config</param>
        /// <param name="root">AST root</param>
        /// <param name="depth">Recursion depth</param>
        /// <returns>Number of dice rolls performed</returns>
        internal ulong Reroll(RollerConfig conf, DiceAST root, uint depth)
        {
            if (!Evaluated)
            {
                return Evaluate(conf, root, depth);
            }

            if (depth > conf.MaxRecursionDepth)
            {
                throw new DiceException(DiceErrorCode.RecursionDepthExceeded, conf.MaxRecursionDepth);
            }

            ulong rolls = RerollInternal(conf, root, depth);

            if (rolls > conf.MaxDice)
            {
                throw new DiceException(DiceErrorCode.TooManyDice, conf.MaxDice);
            }

            return rolls;
        }

        protected abstract ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth);
        protected abstract ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth);
    }
}
