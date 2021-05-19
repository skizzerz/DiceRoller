using System;
using System.Collections.Generic;
using System.Text;

using Dice.AST;

namespace Dice.Builtins
{
    public class RerollData
    {
        public int Current { get; set; }
        public int Max { get; set; }
        public ComparisonNode Comparison { get; private set; }

        public RerollData(int max, IEnumerable<Comparison> comparisons)
        {
            Current = 0;
            Max = max;
            Comparison = new ComparisonNode(comparisons);
        }

        public RerollData(int max, ComparisonNode comparison)
        {
            Current = 0;
            Max = max;
            Comparison = comparison;
        }
    }
}
