using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Contains the context of a macro execution, to be filled out
    /// by the function which is responsible for executing the macro.
    /// </summary>
    public class MacroContext
    {
        /// <summary>
        /// The overall value of the macro.
        /// </summary>
        public decimal Value;

        /// <summary>
        /// If the macro rolls any dice, it should add their results to this list.
        /// If the macro does not roll dice, this need not be touched.
        /// </summary>
        public List<DieResult> Values;

        /// <summary>
        /// The parameter passed to the macro. The function is responsible for
        /// parsing this into something usable.
        /// </summary>
        public readonly string Param;

        internal MacroContext(string param)
        {
            Values = new List<DieResult>();
            Param = param;
        }
    }
}
