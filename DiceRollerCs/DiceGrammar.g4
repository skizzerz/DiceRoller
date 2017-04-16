/* If this file is changed, the corresponding C# files must be regenerated with ANTLR
 * For example: java -jar C:\antlr\antlr-4.7-complete.jar -package Dice.Grammar -o Grammar DiceGrammar.g4
 */
grammar DiceGrammar;
options { language=CSharp; }

input
    : math_expr
    ;

math_expr
    : mult_expr # MathNormal
    | global_function # MathFunction
    ;

mult_expr
    : mult_expr T_MULTIPLY add_expr # MultMult
    | mult_expr T_DIVIDE add_expr # MultDiv
    | add_expr # MultNone
    ;

add_expr
    : add_expr T_PLUS paren_expr # AddAdd
    | add_expr T_MINUS paren_expr # AddSub
    | paren_expr # AddNone
    ;

paren_expr
    : T_LPAREN math_expr T_RPAREN # ParenParen
    | number # ParenNumber
    | grouped_roll # ParenGroup
    ;

number_expr
    : T_LPAREN math_expr T_RPAREN # NumberParen
    | number # NumberNumber
    ;

number
    : T_NUMBER # NumberLiteral
    | T_LSQUARE T_STRING T_RSQUARE # NumberMacro
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

grouped_roll
    : (number_expr)? T_LBRACE grouped_roll_inner T_RBRACE (grouped_extras)* (group_function)* # GroupGroup
    | basic_roll # GroupBasic
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

basic_roll
    : number_expr T_D number_expr (basic_extras)* (basic_function)* # BasicBasic
    | number_expr T_D T_FUDGE (number_expr)? (basic_extras)* (basic_function)* # BasicFudge
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

T_NUMBER : '-'? [0-9]+ ('.' [0-9]+)? ;
T_IDENTIFIER : [a-zA-Z][a-zA-Z0-9]* ;
T_STRING : ~[()[\]{}]+ ;

T_D : 'd' ;
T_FUDGE : 'F' ;

T_KEEP_HIGH : 'kh' ;
T_KEEP_LOW : 'kl' ;
T_DROP_HIGH : 'dh' ;
T_DROP_LOW : 'dl' ;

T_ADVANTAGE : 'ad' ;
T_DISADVANTAGE : 'da' ;

T_REROLL : 'rr' ;
T_REROLL_ONCE : 'ro' ;

T_EXPLODE : '!e' ;
T_COMPOUND : '!c' ;
T_PENETRATE : '!p' ;

T_CRIT : 'cs' ;
T_CRITFAIL : 'cf' ;

/* this token never appears at the beginning of a modifier, as such it is only one letter.
 * tokens which appear at the beginning of modifiers are always two letters or more to allow for disambiguation. */
T_FAIL : 'f' ;

T_SORT_ASC : 'sa' ;
T_SORT_DESC : 'sd' ;

T_EQUALS : '=' ;
T_GREATER : '>' ;
T_LESS : '<' ;
T_GREATER_EQUALS : '>=' ;
T_LESS_EQUALS : '<=' ;
T_NOT_EQUALS : ( '!=' | '<>' ) ;

T_LBRACE : '{' ;
T_RBRACE : '}' ;
T_LSQUARE : '[' ;
T_RSQUARE : ']' ;
T_COMMA : ',' ;
T_DOT : '.' ;

T_LPAREN : '(' ;
T_RPAREN : ')' ;
T_PLUS : '+' ;
T_MINUS : '-' ;
T_MULTIPLY : '*' ;
T_DIVIDE : '/' ;

WS : [\p{White_Space}] -> skip ;
