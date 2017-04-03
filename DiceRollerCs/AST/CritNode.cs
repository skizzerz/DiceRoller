using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Represents marking a die as a critical or fumble, for display purposes.
    /// If no CritNode is explicitly assigned to a die roll, an implicit one is assigned
    /// which marks the lowest roll result as a fumble and the highest roll result as a crit.
    /// </summary>
    public class CritNode : DiceAST
    {
        private List<DieResult> _values;

    }
}
