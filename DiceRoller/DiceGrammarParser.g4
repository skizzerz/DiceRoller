/* If this file is changed, the corresponding C# files must be regenerated with ANTLR
 * For example: java -jar C:\antlr\antlr-4.9.2-complete.jar -package Dice.Grammar -o Grammar/Generated DiceGrammarLexer.g4 DiceGrammarParser.g4
 */

parser grammar DiceGrammarParser;
options { language=CSharp; tokenVocab=DiceGrammarLexer; }

input
    : math_expr EOF
    ;

math_expr
    : add_expr # MathNormal
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
    : (unary_expr)? T_LBRACE grouped_roll_inner T_RBRACE (grouped_extras)* (group_function)* # RollGroup
    | (unary_expr)? T_DIE_BASIC number_expr (basic_extras)* (basic_function)* # RollBasic
    | (unary_expr)? T_DIE_FUDGE (number_expr)? (basic_extras)* (basic_function)* # RollFudge
    | func_expr # RollNone
    ;

func_expr
    : T_MINUS global_function # FuncMinus
    | global_function # FuncFunction
    | unary_expr # FuncNone
    ;

unary_expr
    : T_MINUS number_expr # UnaryExprMinus
    | number_expr # UnaryExprNone
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
    : T_FUNCTION T_LPAREN (function_arg (T_COMMA function_arg)*)? T_RPAREN # GlobalFunction
    ;

group_function
    : T_DOT T_FUNCTION T_LPAREN (function_arg (T_COMMA function_arg)*)? T_RPAREN # GroupFunction
    ;

basic_function
    : T_DOT T_FUNCTION T_LPAREN (function_arg (T_COMMA function_arg)*)? T_RPAREN # BasicFunction
    ;

function_arg
    : math_expr # FnArgMath
    | explicit_compare_expr # FnArgComp
    ;

grouped_roll_inner
    : grouped_roll_inner T_COMMA math_expr # GroupAdditional
    | math_expr # GroupInit
    ;

grouped_extras
    : T_EXTRAS (compare_expr)? # GroupExtra
    ;

basic_extras
    : T_EXTRAS (compare_expr)? # BasicExtra
    ;

compare_expr
    : unary_expr # CompImplicit
    | explicit_compare_expr # CompExplicit
    ;

explicit_compare_expr
    : T_EQUALS unary_expr # CompEquals
    | T_GREATER unary_expr # CompGreater
    | T_LESS unary_expr # CompLess
    | T_GREATER_EQUALS unary_expr # CompGreaterEquals
    | T_LESS_EQUALS unary_expr # CompLessEquals
    | T_NOT_EQUALS unary_expr # CompNotEquals
    ;
