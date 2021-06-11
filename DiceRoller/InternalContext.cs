using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Dice.AST;

namespace Dice
{
    /// <summary>
    /// Contextual information that need not be exposed to library consumers.
    /// </summary>
    internal class InternalContext
    {
        /// <summary>
        /// The result of all dice rolls made, in the order they were rolled.
        /// </summary>
        internal List<uint> AllRolls { get; private set; } = new List<uint>();

        /// <summary>
        /// The result of all macros that were evaluated, in the order of evaluation.
        /// </summary>
        internal List<decimal> AllMacros { get; private set; } = new List<decimal>();

        /// <summary>
        /// All primary roll expressions made as part of this roll.
        /// </summary>
        internal List<RollNode> RollExpressions { get; private set; } = new List<RollNode>();

        /// <summary>
        /// The values of each roll expression made. These values do not change even if the
        /// node is rerolled.
        /// </summary>
        internal List<ValueSnapshot> RollValues { get; private set; } = new List<ValueSnapshot>();

        /// <summary>
        /// All group expressions made as part of this roll.
        /// </summary>
        internal List<DiceAST> GroupExpressions { get; private set; } = new List<DiceAST>();

        /// <summary>
        /// The values of each group expression made. These values do not change even if the
        /// node is rerolled.
        /// </summary>
        internal List<ValueSnapshot> GroupValues { get; private set; } = new List<ValueSnapshot>();

        /// <summary>
        /// Adds a roll expression to the current context.
        /// </summary>
        /// <param name="roll">Expression to add.</param>
        internal void AddRollExpression(RollNode roll)
        {
            RollExpressions.Add(roll);
            RollValues.Add(new ValueSnapshot(roll));
        }

        /// <summary>
        /// Adds a group expression to the current context.
        /// </summary>
        /// <param name="expr">Expression to add.</param>
        /// <returns>The key of the group expression, for later retrieval.</returns>
        internal string AddGroupExpression(DiceAST expr)
        {
            GroupExpressions.Add(expr);
            GroupValues.Add(new ValueSnapshot(expr));
            return (GroupExpressions.Count - 1).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Retrieves the node of a group expression that was previously added.
        /// </summary>
        /// <param name="key">Key to retrieve.</param>
        /// <returns>The GroupNode corresponding to the key.</returns>
        internal DiceAST GetGroupExpression(string key)
        {
            return GroupExpressions[Convert.ToInt32(key, CultureInfo.InvariantCulture)];
        }

        /// <summary>
        /// Retrieves the value snapshot of a group expression that was previously added.
        /// </summary>
        /// <param name="key">Key to retrieve.</param>
        /// <returns>The ValueSnapshot corresponding to the key.</returns>
        internal ValueSnapshot GetGroupValues(string key)
        {
            return GroupValues[Convert.ToInt32(key, CultureInfo.InvariantCulture)];
        }

        /// <summary>
        /// Represents a fixed snapshot of a node's value and underlying dice rolls.
        /// This snapshot does not change even if the node is subsequently rerolled.
        /// It is marked as internal for visibility to outside code but is not meant to
        /// be constructed except inside of InternalContext.
        /// </summary>
        internal class ValueSnapshot
        {
            /// <summary>
            /// The value of this snapshot.
            /// </summary>
            internal decimal Value { get; private set; }

            /// <summary>
            /// The individual dice rolled in this snapshot.
            /// </summary>
            internal IReadOnlyList<DieResult> Values { get; private set; }

            /// <summary>
            /// The type of value this snapshot contains.
            /// </summary>
            internal ValueType ValueType { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ValueSnapshot"/> class.
            /// </summary>
            /// <param name="expr">Expression to snapshot the value of.</param>
            internal ValueSnapshot(DiceAST expr)
            {
                Value = expr.Value;
                Values = expr.Values.ToList();
                ValueType = expr.ValueType;
            }
        }
    }
}
