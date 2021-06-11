using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Dice.Builtins
{
    /// <summary>
    /// Builtin functions related to keeping/dropping dice from a roll.
    /// </summary>
    public static class KeepFunctions
    {
        private enum KeepType
        {
            KeepHighest,
            KeepLowest,
            DropHighest,
            DropLowest
        }

        /// <summary>
        /// Validate that regular keep/drop functions are not intermixed with advantage/disadvantage.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="e">Event args.</param>
        [SuppressMessage("Security", "CA2109:Review visible event handlers",
            Justification = "Public to allow library consumers to remove this validation event from BuiltinFunctionRegistry")]
        public static void ValidateKeep(object sender, ValidateEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            var advantageCount = e.Contexts.Count(c => c.Name == "advantage" || c.Name == "disadvantage");
            var haveOther = e.Contexts.Any(c => c.Name != "advantage" && c.Name != "disadvantage");

            if (advantageCount > 1)
            {
                throw new DiceException(DiceErrorCode.AdvantageOnlyOnce);
            }

            if (advantageCount > 0 && haveOther)
            {
                throw new DiceException(DiceErrorCode.NoAdvantageKeep);
            }
        }

        /// <summary>
        /// Rolls the dice expression twice and keeps the one with the highest result.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("advantage", "ad", Scope = FunctionScope.Roll, Timing = FunctionTiming.Keep, ArgumentPattern = "")]
        public static void Advantage(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ApplyAdvantage(context, KeepType.KeepHighest);
        }

        /// <summary>
        /// Rolls the dice expression twice and keeps the one with the lowest result.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("disadvantage", "da", Scope = FunctionScope.Roll, Timing = FunctionTiming.Keep, ArgumentPattern = "")]
        public static void Disadvantage(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ApplyAdvantage(context, KeepType.KeepLowest);
        }

        /// <summary>
        /// Drops some number of the lowest dice from the roll.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("dropLowest", "dl", Scope = FunctionScope.Roll, Timing = FunctionTiming.Keep, ArgumentPattern = "E")]
        public static void DropLowest(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ApplyKeep(context, KeepType.DropLowest);
        }

        /// <summary>
        /// Drops some number of the highest dice from the roll.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("dropHighest", "dh", Scope = FunctionScope.Roll, Timing = FunctionTiming.Keep, ArgumentPattern = "E")]
        public static void DropHighest(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ApplyKeep(context, KeepType.DropHighest);
        }

        /// <summary>
        /// Keeps some number of the lowest dice from the roll, dropping the rest.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("keepLowest", "kl", Scope = FunctionScope.Roll, Timing = FunctionTiming.Keep, ArgumentPattern = "E")]
        public static void KeepLowest(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ApplyKeep(context, KeepType.KeepLowest);
        }

        /// <summary>
        /// Keeps some number of the highest dice from the roll, dropping the rest.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("keepHighest", "kh", Scope = FunctionScope.Roll, Timing = FunctionTiming.Keep, ArgumentPattern = "E")]
        public static void KeepHighest(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ApplyKeep(context, KeepType.KeepHighest);
        }

        private static void ApplyAdvantage(FunctionContext context, KeepType type)
        {
            // save originally rolled values, then reroll it
            var originalValue = context.Expression!.Value;
            decimal newValue;
            var originalValues = new List<DieResult>(context.Expression!.Values);
            var newValues = new List<DieResult>();
            foreach (var die in originalValues)
            {
                if (die.IsLiveDie())
                {
                    newValues.Add(context.Reroll(die));
                }
                else
                {
                    // keep special dice and dropped dice as-is
                    newValues.Add(die);
                }
            }

            if (context.Expression.ValueType == ResultType.Total)
            {
                newValue = newValues.Sum(d => d.IsLiveDie() ? d.Value : 0);
            }
            else
            {
                newValue = newValues.Sum(d => d.IsLiveDie() ? d.SuccessCount : 0);
            }

            // prefer keeping the original roll on a tie
            var keepOriginal = type switch
            {
                KeepType.KeepHighest => originalValue >= newValue,
                KeepType.KeepLowest => originalValue <= newValue,
                _ => throw new InvalidOperationException("Use only KeepHigh or KeepLow for ApplyAdvantage")
            };

            if (keepOriginal)
            {
                context.Value = originalValue;
                originalValues.Add(new DieResult(SpecialDie.Add));
                originalValues.AddRange(newValues.Select(d => d.Drop()));
            }
            else
            {
                context.Value = newValue;
                originalValues = originalValues.Select(d => d.Drop()).ToList();
                originalValues.Add(new DieResult(SpecialDie.Add));
                originalValues.AddRange(newValues);
            }

            context.Values = originalValues;
            context.ValueType = context.Expression.ValueType;
        }

        private static void ApplyKeep(FunctionContext context, KeepType type)
        {
            var sortedValues = context.Expression!.Values
                .Where(d => d.IsLiveDie())
                .OrderBy(d => d.Value).ToList();
            var amount = (int)context.Arguments[0].Value;

            if (amount < 0)
            {
                throw new DiceException(DiceErrorCode.NegativeDice);
            }

            sortedValues = type switch
            {
                KeepType.DropHighest => sortedValues.Take(sortedValues.Count - amount).ToList(),
                KeepType.KeepLowest => sortedValues.Take(amount).ToList(),
                KeepType.DropLowest => sortedValues.Skip(amount).ToList(),
                KeepType.KeepHighest => sortedValues.Skip(sortedValues.Count - amount).ToList(),
                _ => throw new InvalidOperationException("Unknown keep type"),
            };

            if (context.Expression.ValueType == ResultType.Total)
            {
                context.Value = sortedValues.Sum(d => d.Value);
                context.ValueType = ResultType.Total;
            }
            else
            {
                context.Value = sortedValues.Sum(d => d.SuccessCount);
                context.ValueType = ResultType.Successes;
            }

            var values = new List<DieResult>();
            foreach (var d in context.Expression.Values)
            {
                if (!d.IsLiveDie())
                {
                    // while we apply drop/keep on grouped die results, special dice are passed as-is
                    // also if the die was already dropped, we don't try to drop it again
                    values.Add(d);
                    continue;
                }

                if (sortedValues.Contains(d))
                {
                    values.Add(d);
                    sortedValues.Remove(d);
                }
                else
                {
                    values.Add(d.Drop());
                }
            }

            context.Values = values;
        }
    }
}
