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
        private List<DieResult> _values;

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
            _values = new List<DieResult>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Expression?.ToString() ?? String.Empty);

            switch (Direction)
            {
                case SortDirection.Ascending:
                    sb.Append(".sortAsc()");
                    break;
                case SortDirection.Descending:
                    sb.Append(".sortDesc()");
                    break;
                default:
                    sb.Append(".<<UNKNOWN SORT>>()");
                    break;
            }

            return sb.ToString();
        }

        protected override long EvaluateInternal(RollerConfig conf, DiceAST root, int depth)
        {
            var rolls = Expression.Evaluate(conf, root, depth + 1);
            DoSort();

            return rolls;
        }

        protected override long RerollInternal(RollerConfig conf, DiceAST root, int depth)
        {
            var rolls = Expression.Reroll(conf, root, depth + 1);
            DoSort();

            return rolls;
        }

        internal override DiceAST GetUnderlyingRollNode()
        {
            return Expression.GetUnderlyingRollNode();
        }

        private void DoSort()
        {
            List<DieResult> temp = new List<DieResult>();
            List<int> positions = new List<int>();
            SpecialDie? chainType = null;

            _values = Expression.Values.ToList();
            Value = Expression.Value;
            ValueType = Expression.ValueType;

            void FixOrder()
            {
                if (temp.Count == 0)
                {
                    return;
                }

                IOrderedEnumerable<DieResult> sorted;

                switch (Direction)
                {
                    case SortDirection.Ascending:
                        sorted = temp.OrderBy(d => d.Value);
                        break;
                    case SortDirection.Descending:
                        sorted = temp.OrderByDescending(d => d.Value);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown sort direction");
                }

                var enumerator = sorted.GetEnumerator();
                foreach (var pos in positions)
                {
                    enumerator.MoveNext();
                    _values[pos] = enumerator.Current;
                }

                temp.Clear();
                positions.Clear();
            }

            for (int i = 0; i < _values.Count; i++)
            {
                var die = _values[i];

                if (die.DieType == DieType.Special)
                {
                    switch ((SpecialDie)die.Value)
                    {
                        case SpecialDie.Add:
                            if (chainType == null && temp.Count > 0)
                            {
                                chainType = SpecialDie.Add;
                            }
                            else if (chainType != SpecialDie.Add)
                            {
                                FixOrder();
                            }

                            break;
                        case SpecialDie.Multiply:
                            if (chainType == null && temp.Count > 0)
                            {
                                chainType = SpecialDie.Multiply;
                            }
                            else if (chainType != SpecialDie.Multiply)
                            {
                                FixOrder();
                            }

                            break;
                        default:
                            chainType = null;
                            FixOrder();
                            break;
                    }
                }
                else
                {
                    temp.Add(die);
                    positions.Add(i);
                }
            }

            FixOrder();
        }
    }
}
