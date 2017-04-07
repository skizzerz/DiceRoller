using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Represents sorting the dice rolled into a specific order
    /// </summary>
    public class SortNode : DiceAST
    {
        public List<DieResult> _values;

        /// <summary>
        /// The order the dice are sorted in
        /// </summary>
        public SortDirection Direction { get; private set; }

        /// <summary>
        /// Underlying dice expression to sort
        /// </summary>
        public DiceAST Expression { get; internal set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal SortNode(SortDirection direction)
        {
            Direction = direction;
            Expression = null;
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            var rolls = Expression.Evaluate(conf, root, depth + 1);
            DoSort();

            return rolls;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            var rolls = Expression.Reroll(conf, root, depth + 1);
            DoSort();

            return rolls;
        }

        private void DoSort()
        {
            List<DieResult> temp = new List<DieResult>();
            _values.Clear();
            Value = Expression.Value;

            foreach (var die in Expression.Values)
            {
                if (die.DieType == DieType.Special)
                {
                    if ((SpecialDie)die.Value == SpecialDie.CloseParen)
                    {
                        switch (Direction)
                        {
                            case SortDirection.Ascending:
                                _values.AddRange(temp.OrderBy(d => d.Value));
                                break;
                            case SortDirection.Descending:
                                _values.AddRange(temp.OrderByDescending(d => d.Value));
                                break;
                            default:
                                throw new InvalidOperationException("Unknown sort direction");
                        }

                        temp.Clear();
                    }

                    _values.Add(die);
                }
                else
                {
                    temp.Add(die);
                }
            }

            switch (Direction)
            {
                case SortDirection.Ascending:
                    _values.AddRange(temp.OrderBy(d => d.Value));
                    break;
                case SortDirection.Descending:
                    _values.AddRange(temp.OrderByDescending(d => d.Value));
                    break;
                default:
                    throw new InvalidOperationException("Unknown sort direction");
            }
        }
    }
}
