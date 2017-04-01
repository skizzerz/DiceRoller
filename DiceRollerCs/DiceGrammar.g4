/* If this file is changed, the corresponding C# files must be regenerated with ANTLR
 * For example: java -jar C:\antlr\antlr-4.7-complete.jar -package Dice.Grammar -o Grammar DiceGrammar.g4 */
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
    : T_LPAREN math_expr T_RPAREN # NexprParen
    | number # NexprNumber
    ;

number
    : (T_MINUS)? T_DIGIT_STRING # NumberLiteral
    | T_LSQUARE T_STRING T_RSQUARE # NumberMacro
    ;

global_function
    : T_ALPHA_STRING T_LPAREN T_STRING T_RPAREN
    ;

group_function
    : T_DOT T_ALPHA_STRING T_LPAREN T_STRING T_RPAREN
    ;

basic_function
    : T_DOT T_ALPHA_STRING T_LPAREN T_STRING T_RPAREN
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
    ;

basic_roll
    : number_expr T_D number_expr (basic_extras)* (basic_function)* # BasicBasic
    | number_expr T_D T_FUDGE (number_expr)? # BasicFudge
    ;

basic_extras
    : reroll_expr # BasicReroll
    | explode_expr # BasicExplode
    | keep_expr # BasicKeep
    | success_expr # BasicSuccess
    ;

keep_expr
    : T_KEEP_HIGH number # KeepHigh
    | T_KEEP_LOW number # KeepLow
    | T_DROP_HIGH number # DropHigh
    | T_DROP_LOW number # DropLow
    | T_ADVANTAGE # Advantage
    | T_D # Disadvantage
    ;
    
reroll_expr
    : T_REROLL compare_expr # RerollReroll
    | T_REROLL_ONCE compare_expr # RerollOnce
    ;

explode_expr
    : T_EXPLODE (compare_expr)? # Explode
    | T_PENETRATE (compare_expr)? # Penetrate
    ;

success_expr
    : explicit_compare_expr (T_FAIL compare_expr)?
    ;

compare_expr
    : number # CompImplicit
    | explicit_compare_expr # CompExplicit
    ;

explicit_compare_expr
    : T_EQUALS number # Equals
    | T_GREATER number # Greater
    | T_LESS number # Less
    | T_GREATER_EQUALS number # GreaterEquals
    | T_LESS_EQUALS number # LessEquals
    | T_NOT_EQUALS number # NotEquals
    ;

T_DIGIT_STRING : [0-9]+ ;
T_ALPHA_STRING : [a-zA-Z][a-zA-Z0-9]* ;
T_STRING : ~[()[\]{}]+ ;

T_D : 'd' ;
T_FUDGE : 'F' ;

T_KEEP_HIGH : 'kh' ;
T_KEEP_LOW : 'kl' ;
T_DROP_HIGH : 'dh' ;
T_DROP_LOW : 'dl' ;
T_ADVANTAGE : 'a' ;
/* T_DISADVANTAGE would be 'd', which is T_D. As such, no T_DISADVANTAGE is specified */

T_REROLL : 'r' ;
T_REROLL_ONCE : 'ro' ;

T_EXPLODE : 'e' ;
T_PENETRATE : 'p' ;

T_FAIL : 'f' ;

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
