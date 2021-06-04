using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Holds information about a comparison, which checks a CompareOp against an expression.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "No need for overhead of property, given this is typically only used internally.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter",
        Justification = "This is a backwards compatibility break but will be done in a later major version")]
    public struct Comparison : IEquatable<Comparison>
    {
        /// <summary>
        /// Type of comparison being made.
        /// </summary>
        public CompareOp op;

        /// <summary>
        /// Expression to compare with.
        /// </summary>
        public DiceAST expr;

        /// <summary>
        /// Initializes a new instance of the <see cref="Comparison"/> struct.
        /// </summary>
        /// <param name="op">Type of comparison being made.</param>
        /// <param name="expr">Expression to compare with.</param>
        public Comparison(CompareOp op, DiceAST expr)
        {
            this.op = op;
            this.expr = expr;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is Comparison c)
            {
                return Equals(c);
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Equals(Comparison other)
        {
            return op == other.op && expr == other.expr;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return new { op, expr }.GetHashCode();
        }

        public static bool operator==(Comparison a, Comparison b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(Comparison a, Comparison b)
        {
            return !a.Equals(b);
        }
    }
}
