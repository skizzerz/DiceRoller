#ifndef DICE_AST_H
#define DICE_AST_H

#ifdef __cplusplus
extern "C" {
#endif

#define DICE_NODE_EXTRAS (-1)
#define DICE_NODE_NULL 0
#define DICE_NODE_MATH 1
#define DICE_NODE_GROUP 2
#define DICE_NODE_ROLL 3
#define DICE_NODE_REROLL 4
#define DICE_NODE_EXPLODE 5
#define DICE_NODE_KEEP 6
#define DICE_NODE_SUCCESS 7
#define DICE_NODE_COMPARE 8
#define DICE_NODE_LITERAL 9

#define DICE_OP_ADD 0
#define DICE_OP_SUBTRACT 1
#define DICE_OP_MULTIPLY 2
#define DICE_OP_DIVIDE 3

#define DICE_ROLL_NORMAL 0
#define DICE_ROLL_FATE 1

#define DICE_EXPLODE_EXPLODE 0
#define DICE_EXPLODE_COMPOUND 1
#define DICE_EXPLODE_PENETRATE 2

#define DICE_KEEP_DROP_LOW 0
#define DICE_KEEP_DROP_HIGH 1
#define DICE_KEEP_KEEP_LOW 2
#define DICE_KEEP_KEEP_HIGH 3

#define DICE_COMP_EQUAL 0
#define DICE_COMP_LESSER 1
#define DICE_COMP_GREATER 2

// base type for AST nodes
typedef struct dice_ast_ {
	short type; // node type
	short size; // size (in bytes) of this node
	float value; // value of this node (post evaluation)
} DiceAST;

typedef struct dice_ast_math_ {
	DiceAST base; // value = result of expression
	int op; // operand (+-*/)
	DiceAST *left; // left hand side
	DiceAST *right; // right hand side
} DiceMathNode;

typedef struct dice_ast_group_ {
	DiceAST base; // value = sum of values
	short groupsize; // how many expresions are in the group
	short valuesize; // how many ints are in values
	DiceAST *num; // number of times to evaluate the group
	DiceAST **exprs; // individual expressions in the group (points to an array of groupsize DiceASTs)
	int *values; // stores the value of each expr cast to an int (points to an array of valuesize ints)
} DiceGroupedRollNode;

typedef struct dice_ast_roll_ {
	DiceAST base; // value = sum of values
	short type; // normal or fate
	short valuesize; // how many ints are in values
	DiceAST *num; // number of dice
	DiceAST *sides; // how many sides each die has
	int *values; // the result of the rolls (points to an array of valuesize ints)
} DiceRollNode;

// temporary structure used to aggregate extras attached to a die roll, not kept as a node to the AST
typedef struct dice_ast_extras_ {
	DiceAST base;
	DiceAST *reroll; // DiceRerollNode or NULL
	DiceAST *explode; // DiceExplodeNode or NULL
	DiceAST *keep; // DiceKeepNode or NULL
	DiceAST *success; // DiceSuccessNode or NULL
} DiceExtras;

typedef struct dice_ast_reroll_ {
	DiceAST base; // value = expr->value
	short once; // whether to only reroll once
	short rolled; // number of times this expression was rolled
	DiceAST *cond; // condition to determine if we need a reroll
	DiceAST *expr; // dice expression
} DiceRerollNode;

typedef struct dice_ast_explode_ {
	DiceAST base; // value = expr->value (expr is modified as part of evaluating this)
	short type; // exploding, compounding, penetrating
	short exploded; // number of times this expression exploded
	DiceAST *cond; // condition to determine if we need to explode (if NULL we explode on max value)
	DiceAST *expr; // dice expression
} DiceExplodeNode;

typedef struct dice_ast_keep_ {
	DiceAST base; // value = sum of values
	short type; // keep lowest, keep highest, drop lowest, drop highest
	short num; // number of dice kept
	DiceAST *amount; // amount of dice to keep/drop
	DiceAST *expr; // dice expression
	int *values; // values of kept dice (points to an array of num ints -- note this is the same underlying array as the DiceRollNode)
} DiceKeepNode;

typedef struct dice_ast_success_ {
	DiceAST base; // value = successes - failures
	short successes; // number of successes
	short failures; // number of failures
	DiceAST *success_cond; // success condition
	DiceAST *failure_cond; // failure condition, may be NULL
	DiceAST *expr; // dice expression
} DiceSuccessNode;

typedef struct dice_ast_compare_ {
	DiceAST base; // value = expr->value
	int type; // equal, lesser, greater
	DiceAST *expr; // underlying expression containing value to compare against
} DiceCompareNode;

typedef struct dice_ast_literal_ {
	DiceAST base; // value = literal value
} DiceLiteralNode;

DiceAST *dice_multiply_node(DiceAST *left, DiceAST *right);
DiceAST *dice_divide_node(DiceAST *left, DiceAST *right);
DiceAST *dice_add_node(DiceAST *left, DiceAST *right);
DiceAST *dice_subtract_node(DiceAST *left, DiceAST *right);

DiceAST *dice_literal_node(int value);
DiceAST *dice_nothing();

DiceAST *dice_basic_node(DiceAST *num, DiceAST *sides, DiceAST *extras);
DiceAST *dice_fate_node(DiceAST *num);
DiceAST *dice_group_node(DiceAST *num, DiceAST *group, DiceAST *extras);
DiceAST *dice_group_list(DiceAST *expr);
DiceAST *dice_extend_group(DiceAST *group, DiceAST *expr);

DiceAST *dice_group_extras(DiceAST *keep, DiceAST *success);
DiceAST *dice_basic_extras(DiceAST *reroll, DiceAST *explode, DiceAST *keep, DiceAST *success);

DiceAST *dice_keep_high(DiceAST *amount);
DiceAST *dice_keep_low(DiceAST *amount);
DiceAST *dice_drop_high(DiceAST *amount);
DiceAST *dice_drop_low(DiceAST *amount);

DiceAST *dice_reroll(DiceAST *cond);
DiceAST *dice_reroll_once(DiceAST *cond);

DiceAST *dice_explode(DiceAST *cond);
DiceAST *dice_compound(DiceAST *cond);
DiceAST *dice_penetrate(DiceAST *cond);

DiceAST *dice_success(DiceAST *success_cond, DiceAST *failure_cond);

DiceAST *dice_compare_equals(DiceAST *expr);
DiceAST *dice_compare_greater(DiceAST *expr);
DiceAST *dice_compare_less(DiceAST *expr);

int evaluate(DiceAST *node);
void free_tree(DiceAST *root);
DiceAST *find_roll(DiceAST *node);

#ifdef __cplusplus
}
#endif

#endif
