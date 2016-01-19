%{

#define YY_USE_CONST
#include "dice.h"
#include "dice_ast.h"
#include "dice_parser.h"

%}

%option outfile="dice_lexer.c" header-file="dice_lexer.h"
%option warn nodefault
%option reentrant never-interactive nounistd noyywrap
%option bison-bridge

DIGIT_STRING [0-9]+
SPACE " "

%%

{SPACE}			{ /* no-op, skip blanks */ }
{DIGIT_STRING}	{ yylval->value = strtoul(yytext, NULL, 10); return T_DIGIT_STRING; }

"//".*			|
"--".*			{ /* no-op, skip comments */ }

"d"				{ return T_D; /* used for both drop lowest and dice */ }
"F"				{ return T_FATE; }

"k"				|
"kh"			{ return T_KEEP_HIGH; }
"kl"			{ return T_KEEP_LOW; }
"dl"			{ return T_DROP_LOW; }
"dh"			{ return T_DROP_HIGH; }

"r"				{ return T_REROLL; }
"ro"			{ return T_REROLL_ONCE; }

"!"				{ return T_EXPLODE; }
"!!"			{ return T_COMPOUND; }
"p"				{ return T_PENETRATE; }

"f"				{ return T_FAIL; }

"="				{ return T_EQUALS; }
">"				{ return T_GREATER; }
"<"				{ return T_LESS; }

"{"				{ return T_LBRACE; }
"}"				{ return T_RBRACE; }
"["				{ return T_LSQUARE; }
"]"				{ return T_RSQUARE; }
","				{ return T_COMMA; }

"("				{ return T_LPAREN; }
")"				{ return T_RPAREN; }
"+"				{ return T_PLUS; }
"-"				{ return T_MINUS; }
"*"				{ return T_MULTIPLY; }
"/"				{ return T_DIVIDE; }

.|\n			{ /* no-op */ }

%%
