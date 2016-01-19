%{

#include "dice.h"
#include "dice_ast.h"
#include "dice_parser.h"
#include "dice_lexer.h"

%}

%code requires {

#ifndef YY_TYPEDEF_YY_SCANNER_T
#define YY_TYPEDEF_YY_SCANNER_T
typedef void* yy_scanner_t;
#endif

}

%output "dice_parser.c"
%defines "dice_parser.h"
%expect 9

%define api.pure
%lex-param { yyscan_t scanner }
%parse-param { DiceAST &&expr }
%parse-param { yyscan_t scanner }

%union {
	unsigned int value;
	DiceAST *expr;
}

%token <value> T_DIGIT_STRING
%token T_D
%token T_FATE
%token T_KEEP_HIGH
%token T_KEEP_LOW
%token T_DROP_HIGH
%token T_DROP_LOW
%token T_REROLL
%token T_REROLL_ONCE
%token T_EXPLODE
%token T_COMPOUND
%token T_PENETRATE
%token T_FAIL
%token T_EQUALS
%token T_GREATER
%token T_LESS
%token T_LBRACE
%token T_RBRACE
%token T_LSQUARE
%token T_RSQUARE
%token T_COMMA
%token T_LPAREN
%token T_RPAREN
%token T_PLUS
%token T_MINUS
%token T_MULTIPLY
%token T_DIVIDE

%type <expr> grouped_roll
%type <expr> grouped_roll_inner
%type <expr> grouped_extras
%type <expr> basic_roll
%type <expr> basic_extras
%type <expr> keep_expr
%type <expr> reroll_expr
%type <expr> explode_expr
%type <expr> success_expr
%type <expr> fail_expr
%type <expr> opt_compare_expr
%type <expr> compare_expr
%type <expr> explicit_compare_expr
%type <expr> math_expr
%type <expr> mult_expr
%type <expr> add_expr
%type <expr> paren_expr
%type <expr> number
%type <expr> opt_number

%%

input
	: math_expr { *expr = $1; }
	;

math_expr
	: mult_expr { $$ = $1; }
	;

mult_expr
	: mult_expr T_MULTIPLY add_expr { $$ = dice_multiply_node($1, $3); }
	| mult_expr T_DIVIDE add_expr { $$ = dice_divide_node($1, $3); }
	| add_expr { $$ = $1; }
	;

add_expr
	: add_expr T_PLUS paren_expr { $$ = dice_add_node($1, $3); }
	| add_expr T_MINUS paren_expr { $$ = dice_subtract_node($1, $3); }
	| paren_expr { $$ = $1; }
	;

paren_expr
	: T_LPAREN math_expr T_RPAREN { $$ = $2; }
	| number { $$ = $1; }
	| grouped_roll { $$ = $1; }
	;

number
	: T_DIGIT_STRING { $$ = dice_literal_node($1); }
	| T_LSQUARE math_expr T_RSQUARE { $$ = $2; }
	;

opt_number
	: number { $$ = $1; }
	| %empty { $$ = dice_literal_node(1); }
	;

grouped_roll
	: opt_number T_LBRACE grouped_roll_inner T_RBRACE grouped_extras
		{ $$ = dice_group_node($1, $3, $5); }
	| basic_roll { $$ = $1; }
	;

grouped_roll_inner
	: grouped_roll_inner T_COMMA math_expr { $$ = dice_extend_group($1, $3); }
	| math_expr { $$ = dice_group_list($1); }
	;

grouped_extras
	: keep_expr success_expr { $$ = dice_group_extras($1, $2); }
	;

basic_roll
	: number T_D number basic_extras
		{ $$ = dice_basic_node($1, $3, $4); }
	| number T_D T_FATE
		{ $$ = dice_fate_node($1); }
	;

basic_extras
	: reroll_expr explode_expr keep_expr success_expr { $$ = dice_basic_extras($1, $2, $3, $4); }
	;

keep_expr
	: T_KEEP_HIGH number { $$ = dice_keep_high($2); }
	| T_KEEP_LOW number { $$ = dice_keep_low($2); }
	| T_DROP_HIGH number { $$ = dice_drop_high($2); }
	| T_D number { $$ = dice_drop_low($2); }
	| T_DROP_LOW number { $$ = dice_drop_low($2); }
	| %empty { $$ = dice_nothing(); }
	;
	
reroll_expr
	: T_REROLL compare_expr { $$ = dice_reroll($2); }
	| T_REROLL_ONCE compare_expr { $$ = dice_reroll_once($2); }
	| %empty { $$ = dice_nothing(); }
	;

explode_expr
	: T_EXPLODE opt_compare_expr { $$ = dice_explode($2); }
	| T_COMPOUND opt_compare_expr { $$ = dice_compound($2); }
	| T_PENETRATE opt_compare_expr { $$ = dice_penetrate($2); }
	| %empty { $$ = dice_nothing(); }
	;

success_expr
	: explicit_compare_expr fail_expr { $$ = dice_success($1, $2); }
	| %empty { $$ = dice_nothing(); }
	;

fail_expr
	: T_FAIL compare_expr { $$ = $2; }
	| %empty { $$ = dice_nothing(); }
	;

opt_compare_expr
	: compare_expr { $$ = $1; }
	| %empty { $$ = dice_nothing(); }
	;

compare_expr
	: number { $$ = dice_compare_equals($1); }
	| explicit_compare_expr { $$ = $1; }
	;

explicit_compare_expr
	: T_EQUALS number { $$ = dice_compare_equals($2); }
	| T_GREATER number { $$ = dice_compare_greater($2); }
	| T_LESS number { $$ = dice_compare_less($2); }
	;

%%
