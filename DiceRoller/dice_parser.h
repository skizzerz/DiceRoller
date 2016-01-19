/* A Bison parser, made by GNU Bison 3.0.2.  */

/* Bison interface for Yacc-like parsers in C

   Copyright (C) 1984, 1989-1990, 2000-2013 Free Software Foundation, Inc.

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.  */

/* As a special exception, you may create a larger work that contains
   part or all of the Bison parser skeleton and distribute that work
   under terms of your choice, so long as that work isn't itself a
   parser generator using the skeleton or a modified version thereof
   as a parser skeleton.  Alternatively, if you modify or redistribute
   the parser skeleton itself, you may (at your option) remove this
   special exception, which will cause the skeleton and the resulting
   Bison output files to be licensed under the GNU General Public
   License without this special exception.

   This special exception was added by the Free Software Foundation in
   version 2.2 of Bison.  */

#ifndef YY_YY_DICE_PARSER_H_INCLUDED
# define YY_YY_DICE_PARSER_H_INCLUDED
/* Debug traces.  */
#ifndef YYDEBUG
# define YYDEBUG 0
#endif
#if YYDEBUG
extern int yydebug;
#endif
/* "%code requires" blocks.  */
#line 10 "dice_parser.y" /* yacc.c:1909  */


#ifndef YY_TYPEDEF_YY_SCANNER_T
#define YY_TYPEDEF_YY_SCANNER_T
typedef void* yy_scanner_t;
#endif


#line 53 "dice_parser.h" /* yacc.c:1909  */

/* Token type.  */
#ifndef YYTOKENTYPE
# define YYTOKENTYPE
  enum yytokentype
  {
    T_DIGIT_STRING = 258,
    T_D = 259,
    T_FATE = 260,
    T_KEEP_HIGH = 261,
    T_KEEP_LOW = 262,
    T_DROP_HIGH = 263,
    T_DROP_LOW = 264,
    T_REROLL = 265,
    T_REROLL_ONCE = 266,
    T_EXPLODE = 267,
    T_COMPOUND = 268,
    T_PENETRATE = 269,
    T_FAIL = 270,
    T_EQUALS = 271,
    T_GREATER = 272,
    T_LESS = 273,
    T_LBRACE = 274,
    T_RBRACE = 275,
    T_LSQUARE = 276,
    T_RSQUARE = 277,
    T_COMMA = 278,
    T_LPAREN = 279,
    T_RPAREN = 280,
    T_PLUS = 281,
    T_MINUS = 282,
    T_MULTIPLY = 283,
    T_DIVIDE = 284
  };
#endif

/* Value type.  */
#if ! defined YYSTYPE && ! defined YYSTYPE_IS_DECLARED
typedef union YYSTYPE YYSTYPE;
union YYSTYPE
{
#line 28 "dice_parser.y" /* yacc.c:1909  */

	unsigned int value;
	DiceAST *expr;

#line 100 "dice_parser.h" /* yacc.c:1909  */
};
# define YYSTYPE_IS_TRIVIAL 1
# define YYSTYPE_IS_DECLARED 1
#endif



int yyparse (DiceAST &&expr, yyscan_t scanner);

#endif /* !YY_YY_DICE_PARSER_H_INCLUDED  */
