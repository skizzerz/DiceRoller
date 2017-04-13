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
        public decimal Value { get; set; }

        /// <summary>
        /// What sort of value the macro returns.
        /// </summary>
        public ResultType ValueType { get; set; }

        /// <summary>
        /// If the macro rolls any dice, it should add their results to this list.
        /// If the macro does not roll dice, this need not be touched. If null or empty,
        /// a Literal DieResult will be inserted with its value set to Value.
        /// </summary>
        public IEnumerable<DieResult> Values { get; set; }

        /// <summary>
        /// The parameter passed to the macro. The function is responsible for
        /// parsing this into something usable.
        /// </summary>
        public string Param { get; private set; }

        internal MacroContext(string param)
        {
            Value = Decimal.MinValue;
            Values = null;
            ValueType = ResultType.Total;
            Param = param;
        }
    }
}
