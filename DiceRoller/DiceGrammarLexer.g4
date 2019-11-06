/* If this file is changed, the corresponding C# files must be regenerated with ANTLR
 * For example: java -jar C:\antlr\antlr-4.7.2-complete.jar -package Dice.Grammar -o Grammar DiceGrammarLexer.g4 DiceGrammarParser.g4
 */

lexer grammar DiceGrammarLexer;
options { language=CSharp; }

T_IDENTIFIER : [a-zA-Z][a-zA-Z0-9]* ;
T_GLOBAL_IDENTIFIER : ( [^d][a-zA-Z0-9]* | 'd' [a-zA-Z0-9]+ ) ;
T_NUMBER : [0-9]+ ('.' [0-9]+)? -> pushMode(AFTER_NUMBER) ;
T_MACRO : '[' .+? ']' -> pushMode(AFTER_NUMBER) ;

T_EQUALS : '=' ;
T_GREATER : '>' ;
T_LESS : '<' ;
T_GREATER_EQUALS : '>=' ;
T_LESS_EQUALS : '<=' ;
T_NOT_EQUALS : ( '!=' | '<>' ) ;

T_LPAREN : '(' ;
T_RPAREN : ')' -> pushMode(AFTER_NUMBER) ;
T_LBRACE : '{' ;
T_RBRACE : '}' -> pushMode(AFTER_NUMBER) ;

T_COMMA : ',' ;
T_DOT : '.' ;

T_PLUS : '+' ;
T_MINUS : '-' ;
T_MULTIPLY : '*' ;
T_DIVIDE : '/' ;

WS : [ \r\n\t] -> skip ;

/* AFTER_NUMBER doesn't allow identifiers */
mode AFTER_NUMBER;

AN_NUMBER : [0-9]+ ('.' [0-9]+)? -> type(T_NUMBER) ;
AN_MACRO : '[' .+? ']' -> type(T_MACRO) ;

T_FUDGE : 'F' ;
T_D : 'd' ;

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

AN_EQUALS : '=' -> type(T_EQUALS) ;
AN_GREATER : '>' -> type(T_GREATER) ;
AN_LESS : '<' -> type(T_LESS) ;
AN_GREATER_EQUALS : '>=' -> type(T_GREATER_EQUALS) ;
AN_LESS_EQUALS : '<=' -> type(T_LESS_EQUALS) ;
AN_NOT_EQUALS : ( '!=' | '<>' ) -> type(T_NOT_EQUALS) ;

AN_LBRACE : '{' -> popMode, type(T_LBRACE) ;
AN_RBRACE : '}' -> type(T_RBRACE) ;
AN_COMMA : ',' -> popMode, type(T_COMMA) ;
AN_DOT : '.' -> popMode, type(T_DOT) ;

AN_LPAREN : '(' -> popMode, type(T_LPAREN) ;
AN_RPAREN : ')' -> type(T_RPAREN) ;
AN_PLUS : '+' -> popMode, type(T_PLUS) ;
AN_MINUS : '-' -> popMode, type(T_MINUS) ;
AN_MULTIPLY : '*' -> popMode, type(T_MULTIPLY) ;
AN_DIVIDE : '/' -> popMode, type(T_DIVIDE) ;

AN_WS : [ \r\n\t] -> skip ;