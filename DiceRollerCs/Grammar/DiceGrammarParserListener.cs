//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from DiceGrammarParser.g4 by ANTLR 4.7

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Dice.Grammar {
using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="DiceGrammarParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7")]
[System.CLSCompliant(false)]
public interface IDiceGrammarParserListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="DiceGrammarParser.input"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInput([NotNull] DiceGrammarParser.InputContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DiceGrammarParser.input"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInput([NotNull] DiceGrammarParser.InputContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>MathNormal</c>
	/// labeled alternative in <see cref="DiceGrammarParser.math_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMathNormal([NotNull] DiceGrammarParser.MathNormalContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>MathNormal</c>
	/// labeled alternative in <see cref="DiceGrammarParser.math_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMathNormal([NotNull] DiceGrammarParser.MathNormalContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>MathFunction</c>
	/// labeled alternative in <see cref="DiceGrammarParser.math_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMathFunction([NotNull] DiceGrammarParser.MathFunctionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>MathFunction</c>
	/// labeled alternative in <see cref="DiceGrammarParser.math_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMathFunction([NotNull] DiceGrammarParser.MathFunctionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>MultMult</c>
	/// labeled alternative in <see cref="DiceGrammarParser.mult_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMultMult([NotNull] DiceGrammarParser.MultMultContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>MultMult</c>
	/// labeled alternative in <see cref="DiceGrammarParser.mult_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMultMult([NotNull] DiceGrammarParser.MultMultContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>MultNone</c>
	/// labeled alternative in <see cref="DiceGrammarParser.mult_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMultNone([NotNull] DiceGrammarParser.MultNoneContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>MultNone</c>
	/// labeled alternative in <see cref="DiceGrammarParser.mult_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMultNone([NotNull] DiceGrammarParser.MultNoneContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>MultDiv</c>
	/// labeled alternative in <see cref="DiceGrammarParser.mult_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMultDiv([NotNull] DiceGrammarParser.MultDivContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>MultDiv</c>
	/// labeled alternative in <see cref="DiceGrammarParser.mult_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMultDiv([NotNull] DiceGrammarParser.MultDivContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>AddSub</c>
	/// labeled alternative in <see cref="DiceGrammarParser.add_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAddSub([NotNull] DiceGrammarParser.AddSubContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>AddSub</c>
	/// labeled alternative in <see cref="DiceGrammarParser.add_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAddSub([NotNull] DiceGrammarParser.AddSubContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>AddNone</c>
	/// labeled alternative in <see cref="DiceGrammarParser.add_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAddNone([NotNull] DiceGrammarParser.AddNoneContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>AddNone</c>
	/// labeled alternative in <see cref="DiceGrammarParser.add_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAddNone([NotNull] DiceGrammarParser.AddNoneContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>AddAdd</c>
	/// labeled alternative in <see cref="DiceGrammarParser.add_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAddAdd([NotNull] DiceGrammarParser.AddAddContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>AddAdd</c>
	/// labeled alternative in <see cref="DiceGrammarParser.add_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAddAdd([NotNull] DiceGrammarParser.AddAddContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>RollGroup</c>
	/// labeled alternative in <see cref="DiceGrammarParser.roll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRollGroup([NotNull] DiceGrammarParser.RollGroupContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>RollGroup</c>
	/// labeled alternative in <see cref="DiceGrammarParser.roll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRollGroup([NotNull] DiceGrammarParser.RollGroupContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>RollBasic</c>
	/// labeled alternative in <see cref="DiceGrammarParser.roll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRollBasic([NotNull] DiceGrammarParser.RollBasicContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>RollBasic</c>
	/// labeled alternative in <see cref="DiceGrammarParser.roll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRollBasic([NotNull] DiceGrammarParser.RollBasicContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>RollFudge</c>
	/// labeled alternative in <see cref="DiceGrammarParser.roll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRollFudge([NotNull] DiceGrammarParser.RollFudgeContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>RollFudge</c>
	/// labeled alternative in <see cref="DiceGrammarParser.roll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRollFudge([NotNull] DiceGrammarParser.RollFudgeContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>RollNone</c>
	/// labeled alternative in <see cref="DiceGrammarParser.roll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRollNone([NotNull] DiceGrammarParser.RollNoneContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>RollNone</c>
	/// labeled alternative in <see cref="DiceGrammarParser.roll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRollNone([NotNull] DiceGrammarParser.RollNoneContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>NumberParen</c>
	/// labeled alternative in <see cref="DiceGrammarParser.number_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNumberParen([NotNull] DiceGrammarParser.NumberParenContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>NumberParen</c>
	/// labeled alternative in <see cref="DiceGrammarParser.number_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNumberParen([NotNull] DiceGrammarParser.NumberParenContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>NumberNumber</c>
	/// labeled alternative in <see cref="DiceGrammarParser.number_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNumberNumber([NotNull] DiceGrammarParser.NumberNumberContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>NumberNumber</c>
	/// labeled alternative in <see cref="DiceGrammarParser.number_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNumberNumber([NotNull] DiceGrammarParser.NumberNumberContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>NumberLiteral</c>
	/// labeled alternative in <see cref="DiceGrammarParser.number"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNumberLiteral([NotNull] DiceGrammarParser.NumberLiteralContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>NumberLiteral</c>
	/// labeled alternative in <see cref="DiceGrammarParser.number"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNumberLiteral([NotNull] DiceGrammarParser.NumberLiteralContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>NumberMacro</c>
	/// labeled alternative in <see cref="DiceGrammarParser.number"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNumberMacro([NotNull] DiceGrammarParser.NumberMacroContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>NumberMacro</c>
	/// labeled alternative in <see cref="DiceGrammarParser.number"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNumberMacro([NotNull] DiceGrammarParser.NumberMacroContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>GlobalFunction</c>
	/// labeled alternative in <see cref="DiceGrammarParser.global_function"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterGlobalFunction([NotNull] DiceGrammarParser.GlobalFunctionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>GlobalFunction</c>
	/// labeled alternative in <see cref="DiceGrammarParser.global_function"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitGlobalFunction([NotNull] DiceGrammarParser.GlobalFunctionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>GroupFunction</c>
	/// labeled alternative in <see cref="DiceGrammarParser.group_function"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterGroupFunction([NotNull] DiceGrammarParser.GroupFunctionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>GroupFunction</c>
	/// labeled alternative in <see cref="DiceGrammarParser.group_function"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitGroupFunction([NotNull] DiceGrammarParser.GroupFunctionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BasicFunction</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_function"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasicFunction([NotNull] DiceGrammarParser.BasicFunctionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BasicFunction</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_function"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasicFunction([NotNull] DiceGrammarParser.BasicFunctionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FnArgMath</c>
	/// labeled alternative in <see cref="DiceGrammarParser.function_arg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFnArgMath([NotNull] DiceGrammarParser.FnArgMathContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FnArgMath</c>
	/// labeled alternative in <see cref="DiceGrammarParser.function_arg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFnArgMath([NotNull] DiceGrammarParser.FnArgMathContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FnArgComp</c>
	/// labeled alternative in <see cref="DiceGrammarParser.function_arg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFnArgComp([NotNull] DiceGrammarParser.FnArgCompContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FnArgComp</c>
	/// labeled alternative in <see cref="DiceGrammarParser.function_arg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFnArgComp([NotNull] DiceGrammarParser.FnArgCompContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>GroupInit</c>
	/// labeled alternative in <see cref="DiceGrammarParser.grouped_roll_inner"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterGroupInit([NotNull] DiceGrammarParser.GroupInitContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>GroupInit</c>
	/// labeled alternative in <see cref="DiceGrammarParser.grouped_roll_inner"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitGroupInit([NotNull] DiceGrammarParser.GroupInitContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>GroupExtra</c>
	/// labeled alternative in <see cref="DiceGrammarParser.grouped_roll_inner"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterGroupExtra([NotNull] DiceGrammarParser.GroupExtraContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>GroupExtra</c>
	/// labeled alternative in <see cref="DiceGrammarParser.grouped_roll_inner"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitGroupExtra([NotNull] DiceGrammarParser.GroupExtraContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>GroupKeep</c>
	/// labeled alternative in <see cref="DiceGrammarParser.grouped_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterGroupKeep([NotNull] DiceGrammarParser.GroupKeepContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>GroupKeep</c>
	/// labeled alternative in <see cref="DiceGrammarParser.grouped_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitGroupKeep([NotNull] DiceGrammarParser.GroupKeepContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>GroupSuccess</c>
	/// labeled alternative in <see cref="DiceGrammarParser.grouped_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterGroupSuccess([NotNull] DiceGrammarParser.GroupSuccessContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>GroupSuccess</c>
	/// labeled alternative in <see cref="DiceGrammarParser.grouped_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitGroupSuccess([NotNull] DiceGrammarParser.GroupSuccessContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>GroupSort</c>
	/// labeled alternative in <see cref="DiceGrammarParser.grouped_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterGroupSort([NotNull] DiceGrammarParser.GroupSortContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>GroupSort</c>
	/// labeled alternative in <see cref="DiceGrammarParser.grouped_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitGroupSort([NotNull] DiceGrammarParser.GroupSortContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BasicReroll</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasicReroll([NotNull] DiceGrammarParser.BasicRerollContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BasicReroll</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasicReroll([NotNull] DiceGrammarParser.BasicRerollContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BasicExplode</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasicExplode([NotNull] DiceGrammarParser.BasicExplodeContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BasicExplode</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasicExplode([NotNull] DiceGrammarParser.BasicExplodeContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BasicKeep</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasicKeep([NotNull] DiceGrammarParser.BasicKeepContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BasicKeep</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasicKeep([NotNull] DiceGrammarParser.BasicKeepContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BasicSuccess</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasicSuccess([NotNull] DiceGrammarParser.BasicSuccessContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BasicSuccess</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasicSuccess([NotNull] DiceGrammarParser.BasicSuccessContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BasicSort</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasicSort([NotNull] DiceGrammarParser.BasicSortContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BasicSort</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasicSort([NotNull] DiceGrammarParser.BasicSortContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BasicCrit</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasicCrit([NotNull] DiceGrammarParser.BasicCritContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BasicCrit</c>
	/// labeled alternative in <see cref="DiceGrammarParser.basic_extras"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasicCrit([NotNull] DiceGrammarParser.BasicCritContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>KeepHigh</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterKeepHigh([NotNull] DiceGrammarParser.KeepHighContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>KeepHigh</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitKeepHigh([NotNull] DiceGrammarParser.KeepHighContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>KeepLow</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterKeepLow([NotNull] DiceGrammarParser.KeepLowContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>KeepLow</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitKeepLow([NotNull] DiceGrammarParser.KeepLowContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>DropHigh</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDropHigh([NotNull] DiceGrammarParser.DropHighContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>DropHigh</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDropHigh([NotNull] DiceGrammarParser.DropHighContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>DropLow</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDropLow([NotNull] DiceGrammarParser.DropLowContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>DropLow</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDropLow([NotNull] DiceGrammarParser.DropLowContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Advantage</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAdvantage([NotNull] DiceGrammarParser.AdvantageContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Advantage</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAdvantage([NotNull] DiceGrammarParser.AdvantageContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Disadvantage</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDisadvantage([NotNull] DiceGrammarParser.DisadvantageContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Disadvantage</c>
	/// labeled alternative in <see cref="DiceGrammarParser.keep_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDisadvantage([NotNull] DiceGrammarParser.DisadvantageContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>RerollReroll</c>
	/// labeled alternative in <see cref="DiceGrammarParser.reroll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRerollReroll([NotNull] DiceGrammarParser.RerollRerollContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>RerollReroll</c>
	/// labeled alternative in <see cref="DiceGrammarParser.reroll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRerollReroll([NotNull] DiceGrammarParser.RerollRerollContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>RerollOnce</c>
	/// labeled alternative in <see cref="DiceGrammarParser.reroll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRerollOnce([NotNull] DiceGrammarParser.RerollOnceContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>RerollOnce</c>
	/// labeled alternative in <see cref="DiceGrammarParser.reroll_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRerollOnce([NotNull] DiceGrammarParser.RerollOnceContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Explode</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explode_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExplode([NotNull] DiceGrammarParser.ExplodeContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Explode</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explode_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExplode([NotNull] DiceGrammarParser.ExplodeContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Compound</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explode_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompound([NotNull] DiceGrammarParser.CompoundContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Compound</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explode_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompound([NotNull] DiceGrammarParser.CompoundContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Penetrate</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explode_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPenetrate([NotNull] DiceGrammarParser.PenetrateContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Penetrate</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explode_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPenetrate([NotNull] DiceGrammarParser.PenetrateContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>SuccessFail</c>
	/// labeled alternative in <see cref="DiceGrammarParser.success_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSuccessFail([NotNull] DiceGrammarParser.SuccessFailContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>SuccessFail</c>
	/// labeled alternative in <see cref="DiceGrammarParser.success_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSuccessFail([NotNull] DiceGrammarParser.SuccessFailContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>CompImplicit</c>
	/// labeled alternative in <see cref="DiceGrammarParser.compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompImplicit([NotNull] DiceGrammarParser.CompImplicitContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>CompImplicit</c>
	/// labeled alternative in <see cref="DiceGrammarParser.compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompImplicit([NotNull] DiceGrammarParser.CompImplicitContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>CompExplicit</c>
	/// labeled alternative in <see cref="DiceGrammarParser.compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompExplicit([NotNull] DiceGrammarParser.CompExplicitContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>CompExplicit</c>
	/// labeled alternative in <see cref="DiceGrammarParser.compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompExplicit([NotNull] DiceGrammarParser.CompExplicitContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>CompEquals</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompEquals([NotNull] DiceGrammarParser.CompEqualsContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>CompEquals</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompEquals([NotNull] DiceGrammarParser.CompEqualsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>CompGreater</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompGreater([NotNull] DiceGrammarParser.CompGreaterContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>CompGreater</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompGreater([NotNull] DiceGrammarParser.CompGreaterContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>CompLess</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompLess([NotNull] DiceGrammarParser.CompLessContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>CompLess</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompLess([NotNull] DiceGrammarParser.CompLessContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>CompGreaterEquals</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompGreaterEquals([NotNull] DiceGrammarParser.CompGreaterEqualsContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>CompGreaterEquals</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompGreaterEquals([NotNull] DiceGrammarParser.CompGreaterEqualsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>CompLessEquals</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompLessEquals([NotNull] DiceGrammarParser.CompLessEqualsContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>CompLessEquals</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompLessEquals([NotNull] DiceGrammarParser.CompLessEqualsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>CompNotEquals</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompNotEquals([NotNull] DiceGrammarParser.CompNotEqualsContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>CompNotEquals</c>
	/// labeled alternative in <see cref="DiceGrammarParser.explicit_compare_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompNotEquals([NotNull] DiceGrammarParser.CompNotEqualsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>SortAsc</c>
	/// labeled alternative in <see cref="DiceGrammarParser.sort_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSortAsc([NotNull] DiceGrammarParser.SortAscContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>SortAsc</c>
	/// labeled alternative in <see cref="DiceGrammarParser.sort_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSortAsc([NotNull] DiceGrammarParser.SortAscContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>SortDesc</c>
	/// labeled alternative in <see cref="DiceGrammarParser.sort_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSortDesc([NotNull] DiceGrammarParser.SortDescContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>SortDesc</c>
	/// labeled alternative in <see cref="DiceGrammarParser.sort_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSortDesc([NotNull] DiceGrammarParser.SortDescContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>CritFumble</c>
	/// labeled alternative in <see cref="DiceGrammarParser.crit_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCritFumble([NotNull] DiceGrammarParser.CritFumbleContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>CritFumble</c>
	/// labeled alternative in <see cref="DiceGrammarParser.crit_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCritFumble([NotNull] DiceGrammarParser.CritFumbleContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FumbleOnly</c>
	/// labeled alternative in <see cref="DiceGrammarParser.crit_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFumbleOnly([NotNull] DiceGrammarParser.FumbleOnlyContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FumbleOnly</c>
	/// labeled alternative in <see cref="DiceGrammarParser.crit_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFumbleOnly([NotNull] DiceGrammarParser.FumbleOnlyContext context);
}
} // namespace Dice.Grammar