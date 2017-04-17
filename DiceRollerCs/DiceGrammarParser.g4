/* If this file is changed, the corresponding C# files must be regenerated with ANTLR
 * For example: java -jar C:\antlr\antlr-4.7-complete.jar -package Dice.Grammar -o Grammar DiceGrammarLexer.g4 DiceGrammarParser.g4
 */

parser grammar DiceGrammarParser;
options { language=CSharp; tokenVocab=DiceGrammarLexer; }

input
    : math_expr EOF
    ;

math_expr
    : add_expr # MathNormal
    | global_function # MathFunction
    ;

add_expr
    : add_expr T_PLUS mult_expr # AddAdd
    | add_expr T_MINUS mult_expr # AddSub
    | mult_expr # AddNone
    ;

mult_expr
    : mult_expr T_MULTIPLY roll_expr # MultMult
    | mult_expr T_DIVIDE roll_expr # MultDiv
    | roll_expr # MultNone
    ;

roll_expr
    : (number_expr)? T_LBRACE grouped_roll_inner T_RBRACE (grouped_extras)* (group_function)* # RollGroup
    | number_expr T_D number_expr (basic_extras)* (basic_function)* # RollBasic
    | number_expr T_D T_FUDGE (number_expr)? (basic_extras)* (basic_function)* # RollFudge
    | number_expr # RollNone
    ;

number_expr
    : T_LPAREN math_expr T_RPAREN # NumberParen
    | number # NumberNumber
    ;

number
    : T_NUMBER # NumberLiteral
    | T_MACRO # NumberMacro
    ;

global_function
    : T_IDENTIFIER T_LPAREN (function_arg (T_COMMA function_arg)*)? T_RPAREN # GlobalFunction
    ;

group_function
    : T_DOT T_IDENTIFIER T_LPAREN (function_arg (T_COMMA function_arg)*)? T_RPAREN # GroupFunction
    ;

basic_function
    : T_DOT T_IDENTIFIER T_LPAREN (function_arg (T_COMMA function_arg)*)? T_RPAREN # BasicFunction
    ;

function_arg
    : math_expr # FnArgMath
    | explicit_compare_expr # FnArgComp
    ;

grouped_roll_inner
    : grouped_roll_inner T_COMMA math_expr # GroupExtra
    | math_expr # GroupInit
    ;

grouped_extras
    : keep_expr # GroupKeep
    | success_expr # GroupSuccess
    | sort_expr # GroupSort
    ;

basic_extras
    : reroll_expr # BasicReroll
    | explode_expr # BasicExplode
    | keep_expr # BasicKeep
    | success_expr # BasicSuccess
    | sort_expr # BasicSort
    | crit_expr # BasicCrit
    ;

keep_expr
    : T_KEEP_HIGH number # KeepHigh
    | T_KEEP_LOW number # KeepLow
    | T_DROP_HIGH number # DropHigh
    | T_DROP_LOW number # DropLow
    | T_ADVANTAGE # Advantage
    | T_DISADVANTAGE # Disadvantage
    ;
    
reroll_expr
    : T_REROLL compare_expr # RerollReroll
    | T_REROLL_ONCE compare_expr # RerollOnce
    ;

explode_expr
    : T_EXPLODE (compare_expr)? # Explode
    | T_COMPOUND (compare_expr)? # Compound
    | T_PENETRATE (compare_expr)? # Penetrate
    ;

success_expr
    : explicit_compare_expr (T_FAIL compare_expr)? # SuccessFail
    ;

compare_expr
    : number # CompImplicit
    | explicit_compare_expr # CompExplicit
    ;

explicit_compare_expr
    : T_EQUALS number # CompEquals
    | T_GREATER number # CompGreater
    | T_LESS number # CompLess
    | T_GREATER_EQUALS number # CompGreaterEquals
    | T_LESS_EQUALS number # CompLessEquals
    | T_NOT_EQUALS number # CompNotEquals
    ;

sort_expr
    : T_SORT_ASC # SortAsc
    | T_SORT_DESC # SortDesc
    ;

crit_expr
    : T_CRIT compare_expr (T_FAIL compare_expr)? # CritFumble
    | T_CRITFAIL compare_expr # FumbleOnly
    ;
