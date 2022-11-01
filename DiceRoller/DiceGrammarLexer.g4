/* If this file is changed, the corresponding C# files must be regenerated with ANTLR
 * For example: java -jar C:\antlr\antlr-4.11.1-complete.jar -package Dice.Grammar -o Grammar/Generated DiceGrammarLexer.g4 DiceGrammarParser.g4
 */

lexer grammar DiceGrammarLexer;
options { language=CSharp; }

T_EQUALS : '=' ;
T_GREATER : '>' ;
T_LESS : '<' ;
T_GREATER_EQUALS : '>=' ;
T_LESS_EQUALS : '<=' ;
T_NOT_EQUALS : ( '!=' | '<>' ) ;

T_LPAREN : '(' -> pushMode(DEFAULT_MODE) ;
T_RPAREN : ')' -> popMode ;
T_LBRACE : '{' ;
T_RBRACE : '}' -> pushMode(ALLOW_EXTRAS) ;

T_COMMA : ',' ;
T_DOT : '.' ;

T_PLUS : '+' ;
T_MINUS : '-' ;
T_MULTIPLY : '*' ;
T_DIVIDE : '/' ;

T_MACRO : '[' .+? ']' ;
T_NUMBER : [0-9]+ ('.' [0-9]+)? ;
T_FUNCTION : [a-zA-Z] [a-zA-Z0-9]* { InputStream.LA(1) == '(' && !IsLikelyDiceExpression() }? ;

T_DIE_BASIC : ( 'd' | 'D' ) -> pushMode(ALLOW_EXTRAS) ;
T_DIE_FUDGE : ( 'd' | 'D' ) ( 'f' | 'F' ) -> pushMode(ALLOW_EXTRAS) ;

T_WS : [ \t\r\n]+ -> skip ;

mode ALLOW_EXTRAS;

AE_EQUALS : '=' -> type(T_EQUALS) ;
AE_GREATER : '>' -> type(T_GREATER) ;
AE_LESS : '<' -> type(T_LESS) ;
AE_GREATER_EQUALS : '>=' -> type(T_GREATER_EQUALS) ;
AE_LESS_EQUALS : '<=' -> type(T_LESS_EQUALS) ;
AE_NOT_EQUALS : ( '!=' | '<>' ) -> type(T_NOT_EQUALS) ;

AE_LPAREN : '(' -> pushMode(DEFAULT_MODE), type(T_LPAREN) ;
AE_RPAREN : ')' -> popMode, type(T_RPAREN) ;
AE_LBRACE : '{' -> popMode, type(T_LBRACE);
AE_RBRACE : '}' -> type(T_RBRACE) ;

AE_COMMA : ',' -> popMode, type(T_COMMA) ;
AE_DOT : '.' -> popMode, type(T_DOT) ;

AE_PLUS : '+' -> popMode, type(T_PLUS) ;
AE_MINUS : '-' -> popMode, type(T_MINUS) ;
AE_MULTIPLY : '*' -> popMode, type(T_MULTIPLY) ;
AE_DIVIDE : '/' -> popMode, type(T_DIVIDE) ;

AE_MACRO : '[' .+? ']' -> type(T_MACRO) ;
AE_NUMBER : [0-9]+ ('.' [0-9]+)? -> type(T_NUMBER) ;

/* T_FUNCTION is not allowed inside of ALLOW_EXTRAS; we instead lex for T_EXTRAS.
  If we see ! at the end of an extra, prefer making that part of a != (T_NOT_EQUALS)
  rather than rolling it into T_EXTRAS. */
T_EXTRAS : [a-zA-Z!] [a-zA-Z!]* { !Text.EndsWith("!") || InputStream.LA(1) != '=' }? ;

AE_WS : [ \t\r\n]+ -> skip ;
