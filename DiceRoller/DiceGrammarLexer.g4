/* If this file is changed, the corresponding C# files must be regenerated with ANTLR
 * For example: java -jar C:\antlr\antlr-4.9.2-complete.jar -package Dice.Grammar -o Grammar DiceGrammarLexer.g4 DiceGrammarParser.g4
 */

lexer grammar DiceGrammarLexer;
options { language=CSharp; }

T_EQUALS : '=' ;
T_GREATER : '>' ;
T_LESS : '<' ;
T_GREATER_EQUALS : '>=' ;
T_LESS_EQUALS : '<=' ;
T_NOT_EQUALS : ( '!=' | '<>' ) ;

T_LPAREN : '(' ;
T_RPAREN : ')' ;
T_LBRACE : '{' ;
T_RBRACE : '}' ;

T_COMMA : ',' ;
T_DOT : '.'

T_PLUS : '+' ;
T_MINUS : '-' ;
T_MULTIPLY : '*' ;
T_DIVIDE : '/' ;

T_D : 'd' ;
T_FUDGE : 'F' ;

T_MACRO : '[' .+? ']' ;
T_NUMBER : [0-9]+ ('.' [0-9]+)? ;
T_GLOBAL_FUNCTION : [a-zA-Z] [a-zA-Z0-9]* {  }? ;
