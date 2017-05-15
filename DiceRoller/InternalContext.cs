using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dice.AST;

namespace Dice
{
    internal class InternalContext
    {
        internal List<uint> AllRolls = new List<uint>();
        internal List<decimal> AllMacros = new List<decimal>();
        internal List<DiceAST> GroupExpressions = new List<DiceAST>();

        internal string AddGroupExpression(DiceAST expr)
        {
            GroupExpressions.Add(expr);
            return (GroupExpressions.Count - 1).ToString();
        }

        internal DiceAST GetGroupExpression(string key)
        {
            return GroupExpressions[Convert.ToInt32(key)];
        }
    }
}
