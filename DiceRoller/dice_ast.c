#include <stdlib.h>
#include <assert.h>
#include "dice_ast.h"

DiceAST *dice_multiply_node(DiceAST *left, DiceAST *right)
{
	DiceMathNode *node;
	
	node = (DiceMathNode *)malloc(sizeof(DiceMathNode));
	node->base.type = DICE_NODE_MATH;
	node->base.size = sizeof(DiceMathNode);
	node->base.value = 0;
	node->op = DICE_OP_MULTIPLY;
	node->left = left;
	node->right = right;

	return (DiceAST *)node;
}

DiceAST *dice_divide_node(DiceAST *left, DiceAST *right)
{
	DiceMathNode *node;
	
	node = (DiceMathNode *)malloc(sizeof(DiceMathNode));
	node->base.type = DICE_NODE_MATH;
	node->base.size = sizeof(DiceMathNode);
	node->base.value = 0;
	node->op = DICE_OP_DIVIDE;
	node->left = left;
	node->right = right;

	return (DiceAST *)node;
}

DiceAST *dice_add_node(DiceAST *left, DiceAST *right)
{
	DiceMathNode *node;
	
	node = (DiceMathNode *)malloc(sizeof(DiceMathNode));
	node->base.type = DICE_NODE_MATH;
	node->base.size = sizeof(DiceMathNode);
	node->base.value = 0;
	node->op = DICE_OP_ADD;
	node->left = left;
	node->right = right;

	return (DiceAST *)node;
}

DiceAST *dice_subtract_node(DiceAST *left, DiceAST *right)
{
	DiceMathNode *node;
	
	node = (DiceMathNode *)malloc(sizeof(DiceMathNode));
	node->base.type = DICE_NODE_MATH;
	node->base.size = sizeof(DiceMathNode);
	node->base.value = 0;
	node->op = DICE_OP_SUBTRACT;
	node->left = left;
	node->right = right;

	return (DiceAST *)node;
}

DiceAST *dice_literal_node(int value)
{
	DiceLiteralNode *node;
	
	node = (DiceLiteralNode *)malloc(sizeof(DiceLiteralNode));
	node->base.type = DICE_NODE_LITERAL;
	node->base.size = sizeof(DiceLiteralNode);
	node->base.value = value;

	return (DiceAST *)node;
}

DiceAST *dice_nothing()
{
	DiceAST *node;
	
	node = (DiceAST *)malloc(sizeof(DiceAST));
	node->type = DICE_NODE_NULL;
	node->size = sizeof(DiceAST);
	node->value = 0;
}

DiceAST *dice_basic_node(DiceAST *num, DiceAST *sides, DiceAST *extras)
{
	DiceAST *ret;
	DiceExtras *ext;
	DiceRollNode *node;
	DiceKeepNode *keep;
	DiceExplodeNode *explode;
	DiceRerollNode *reroll;
	DiceSuccessNode *success;

	node = (DiceRollNode *)malloc(sizeof(DiceRollNode));
	node->base.type = DICE_NODE_ROLL;
	node->base.size = sizeof(DiceRollNode);
	node->base.value = 0;
	node->type = DICE_ROLL_NORMAL;
	node->extra = 0;
	node->num = num;
	node->sides = sides;
	node->values = NULL;
	ret = (DiceAST *)node;

	if (extras->type == DICE_NODE_EXTRAS)
	{
		ext = (DiceExtras *)extras;

		if (ext->reroll != NULL)
		{
			assert(ext->reroll->type == DICE_NODE_REROLL);
			((DiceRerollNode *)ext->reroll)->expr = ret;
			ret = ext->reroll;
		}

		if (ext->explode != NULL)
		{
			assert(ext->explode->type == DICE_NODE_EXPLODE);
			((DiceExplodeNode *)ext->explode)->expr = ret;
			ret = ext->explode;
		}

		if (ext->keep != NULL)
		{
			assert(ext->keep->type == DICE_NODE_KEEP);
			((DiceKeepNode *)ext->keep)->expr = ret;
			ret = ext->keep;
		}

		if (ext->success != NULL)
		{
			assert(ext->success->type == DICE_NODE_SUCCESS);
			((DiceSuccessNode *)ext->success)->expr = ret;
			ret = ext->success;
		}
	}

	free(extras);

	return ret;
}

DiceAST *dice_fate_node(DiceAST *num, DiceAST *extras)
{
	DiceAST *ret;
	DiceExtras *ext;
	DiceRollNode *node;

	node = (DiceRollNode *)malloc(sizeof(DiceRollNode));
	node->base.type = DICE_NODE_ROLL;
	node->base.size = sizeof(DiceRollNode);
	node->base.value = 0;
	node->type = DICE_ROLL_FATE;
	node->extra = 0;
	node->num = num;
	node->sides = NULL;
	node->values = NULL;
	ret = (DiceAST *)node;

	if (extras->type == DICE_NODE_EXTRAS)
	{
		ext = (DiceExtras *)extras;

		if (ext->reroll != NULL)
		{
			assert(ext->reroll->type == DICE_NODE_REROLL);
			((DiceRerollNode *)ext->reroll)->expr = ret;
			ret = ext->reroll;
		}

		if (ext->explode != NULL)
		{
			assert(ext->explode->type == DICE_NODE_EXPLODE);
			((DiceExplodeNode *)ext->explode)->expr = ret;
			ret = ext->explode;
		}

		if (ext->keep != NULL)
		{
			assert(ext->keep->type == DICE_NODE_KEEP);
			((DiceKeepNode *)ext->keep)->expr = ret;
			ret = ext->keep;
		}

		if (ext->success != NULL)
		{
			assert(ext->success->type == DICE_NODE_SUCCESS);
			((DiceSuccessNode *)ext->success)->expr = ret;
			ret = ext->success;
		}
	}

	free(extras);

	return ret;
}

DiceAST *dice_group_node(DiceAST *num, DiceAST *group, DiceAST *extras)
{
	DiceAST *ret;
	DiceExtras *ext;
	DiceGroupedRollNode *node;

	assert(group->type == DICE_NODE_GROUP);
	node = (DiceGroupedRollNode *)group;
	node->num = num;
	ret = group;

	if (extras->type == DICE_NODE_EXTRAS) {
		ext = (DiceExtras *)extras;
		assert(ext->reroll == NULL);
		assert(ext->explode == NULL);

		if (ext->keep != NULL) {
			assert(ext->keep->type == DICE_NODE_KEEP);
			((DiceKeepNode *)ext->keep)->expr = ret;
			ret = ext->keep;
		}

		if (ext->success != NULL) {
			assert(ext->success->type == DICE_NODE_SUCCESS);
			((DiceSuccessNode *)ext->success)->expr = ret;
			ret = ext->success;
		}
	}

	free(extras);

	return ret;
}

DiceAST *dice_group_list(DiceAST *expr)
{
	DiceGroupedRollNode *node;
	DiceAST **list;

	list = (DiceAST **)calloc(1, sizeof(DiceAST *));
	list[0] = expr;
	node = (DiceGroupedRollNode *)malloc(sizeof(DiceGroupedRollNode));
	node->base.type = DICE_NODE_GROUP;
	node->base.size = sizeof(DiceGroupedRollNode);
	node->base.value = 0;
	node->num = NULL;
	node->values = NULL;
	node->valuesize = 0;
	node->groupsize = 1;
	node->exprs = list;

	return (DiceAST *)node;
}

DiceAST *dice_extend_group(DiceAST *group, DiceAST *expr)
{
	DiceGroupedRollNode *node;

	assert(group->type == DICE_NODE_GROUP);
	node = (DiceGroupedRollNode *)group;
	node->groupsize += 1;
	node->exprs = (DiceAST **)realloc(node->exprs, node->groupsize * sizeof(DiceAST *));
	node->exprs[node->groupsize - 1] = expr;

	return group;
}

DiceAST *dice_group_extras(DiceAST *keep, DiceAST *success)
{
	DiceExtras *node;

	node = (DiceExtras *)malloc(sizeof(DiceExtras));
	node->base.type = DICE_NODE_EXTRAS;
	node->base.size = sizeof(DiceExtras);
	node->base.value = 0;
	node->explode = NULL;
	node->reroll = NULL;
	node->keep = keep;
	node->success = success;

	if (keep->type == DICE_NODE_NULL)
	{
		node->keep = NULL;
		free(keep);
	}

	if (success->type == DICE_NODE_NULL)
	{
		node->success = NULL;
		free(success);
	}

	return (DiceAST *)node;
}

DiceAST *dice_basic_extras(DiceAST *reroll, DiceAST *explode, DiceAST *keep, DiceAST *success)
{
	DiceExtras *node;

	node = (DiceExtras *)malloc(sizeof(DiceExtras));
	node->base.type = DICE_NODE_EXTRAS;
	node->base.size = sizeof(DiceExtras);
	node->base.value = 0;
	node->explode = explode;
	node->reroll = reroll;
	node->keep = keep;
	node->success = success;

	if (explode->type == DICE_NODE_NULL)
	{
		node->explode = NULL;
		free(explode);
	}

	if (reroll->type == DICE_NODE_NULL)
	{
		node->reroll = NULL;
		free(reroll);
	}

	if (keep->type == DICE_NODE_NULL)
	{
		node->keep = NULL;
		free(keep);
	}

	if (success->type == DICE_NODE_NULL)
	{
		node->success = NULL;
		free(success);
	}

	return (DiceAST *)node;
}

DiceAST *dice_keep_high(DiceAST *amount) {
	DiceKeepNode *node;

	node = (DiceKeepNode *)malloc(sizeof(DiceKeepNode));
	node->base.type = DICE_NODE_KEEP;
	node->base.size = sizeof(DiceKeepNode);
	node->base.value = 0;
	node->amount = amount;
	node->type = DICE_KEEP_KEEP_HIGH;
	node->expr = NULL;
	node->num = 0;
	node->values = NULL;

	return (DiceAST *)node;
}

DiceAST *dice_keep_low(DiceAST *amount) {
	DiceKeepNode *node;

	node = (DiceKeepNode *)malloc(sizeof(DiceKeepNode));
	node->base.type = DICE_NODE_KEEP;
	node->base.size = sizeof(DiceKeepNode);
	node->base.value = 0;
	node->amount = amount;
	node->type = DICE_KEEP_KEEP_LOW;
	node->expr = NULL;
	node->num = 0;
	node->values = NULL;

	return (DiceAST *)node;
}

DiceAST *dice_drop_high(DiceAST *amount) {
	DiceKeepNode *node;

	node = (DiceKeepNode *)malloc(sizeof(DiceKeepNode));
	node->base.type = DICE_NODE_KEEP;
	node->base.size = sizeof(DiceKeepNode);
	node->base.value = 0;
	node->amount = amount;
	node->type = DICE_KEEP_DROP_HIGH;
	node->expr = NULL;
	node->num = 0;
	node->values = NULL;

	return (DiceAST *)node;
}

DiceAST *dice_drop_low(DiceAST *amount) {
	DiceKeepNode *node;

	node = (DiceKeepNode *)malloc(sizeof(DiceKeepNode));
	node->base.type = DICE_NODE_KEEP;
	node->base.size = sizeof(DiceKeepNode);
	node->base.value = 0;
	node->amount = amount;
	node->type = DICE_KEEP_DROP_LOW;
	node->expr = NULL;
	node->num = 0;
	node->values = NULL;

	return (DiceAST *)node;
}

DiceAST *dice_reroll(DiceAST *cond) {
	DiceRerollNode *node;

	node = (DiceRerollNode *)malloc(sizeof(DiceRerollNode));
	node->base.type = DICE_NODE_REROLL;
	node->base.size = sizeof(DiceRerollNode);
	node->base.value = 0;
	node->once = 0;
	node->cond = cond;
	node->expr = NULL;
	node->rerolled = 0;

	return (DiceAST *)node;
}

DiceAST *dice_reroll_once(DiceAST *cond) {
	DiceRerollNode *node;

	node = (DiceRerollNode *)malloc(sizeof(DiceRerollNode));
	node->base.type = DICE_NODE_REROLL;
	node->base.size = sizeof(DiceRerollNode);
	node->base.value = 0;
	node->once = 1;
	node->cond = cond;
	node->expr = NULL;
	node->rerolled = 0;

	return (DiceAST *)node;
}

DiceAST *dice_explode(DiceAST *cond) {
	DiceExplodeNode *node;

	node = (DiceExplodeNode *)malloc(sizeof(DiceExplodeNode));
	node->base.type = DICE_NODE_EXPLODE;
	node->base.size = sizeof(DiceExplodeNode);
	node->base.value = 0;
	node->type = DICE_EXPLODE_EXPLODE;
	node->cond = cond;
	node->expr = NULL;
	node->exploded = 0;

	return (DiceAST *)node;
}

DiceAST *dice_compound(DiceAST *cond) {
	DiceExplodeNode *node;

	node = (DiceExplodeNode *)malloc(sizeof(DiceExplodeNode));
	node->base.type = DICE_NODE_EXPLODE;
	node->base.size = sizeof(DiceExplodeNode);
	node->base.value = 0;
	node->type = DICE_EXPLODE_COMPOUND;
	node->cond = cond;
	node->expr = NULL;
	node->exploded = 0;

	return (DiceAST *)node;
}

DiceAST *dice_penetrate(DiceAST *cond) {
	DiceExplodeNode *node;

	node = (DiceExplodeNode *)malloc(sizeof(DiceExplodeNode));
	node->base.type = DICE_NODE_EXPLODE;
	node->base.size = sizeof(DiceExplodeNode);
	node->base.value = 0;
	node->type = DICE_EXPLODE_PENETRATE;
	node->cond = cond;
	node->expr = NULL;
	node->exploded = 0;

	return (DiceAST *)node;
}

DiceAST *dice_success(DiceAST *success_cond, DiceAST *failure_cond) {
	DiceSuccessNode *node;

	node = (DiceSuccessNode *)malloc(sizeof(DiceSuccessNode));
	node->base.type = DICE_NODE_SUCCESS;
	node->base.size = sizeof(DiceSuccessNode);
	node->base.value = 0;
	node->success_cond = success_cond;
	node->failure_cond = failure_cond;
	node->successes = 0;
	node->failures = 0;

	if (success_cond != NULL && success_cond->type == DICE_NODE_NULL)
		node->success_cond = NULL;

	if (failure_cond != NULL && failure_cond->type == DICE_NODE_NULL)
		node->failure_cond = NULL;

	return (DiceAST *)node;
}

DiceAST *dice_compare_equals(DiceAST *expr) {
	DiceCompareNode *node;

	node = (DiceCompareNode *)malloc(sizeof(DiceCompareNode));
	node->base.type = DICE_NODE_COMPARE;
	node->base.size = sizeof(DiceCompareNode);
	node->base.value = 0;
	node->type = DICE_COMP_EQUAL;
	node->expr = expr;

	return (DiceAST *)node;
}

DiceAST *dice_compare_greater(DiceAST *expr) {
	DiceCompareNode *node;

	node = (DiceCompareNode *)malloc(sizeof(DiceCompareNode));
	node->base.type = DICE_NODE_COMPARE;
	node->base.size = sizeof(DiceCompareNode);
	node->base.value = 0;
	node->type = DICE_COMP_GREATER;
	node->expr = expr;

	return (DiceAST *)node;
}

DiceAST *dice_compare_less(DiceAST *expr) {
	DiceCompareNode *node;

	node = (DiceCompareNode *)malloc(sizeof(DiceCompareNode));
	node->base.type = DICE_NODE_COMPARE;
	node->base.size = sizeof(DiceCompareNode);
	node->base.value = 0;
	node->type = DICE_COMP_LESSER;
	node->expr = expr;

	return (DiceAST *)node;
}

// produces a random integer from [1, max] (inclusive)
static int rand_int(int max) {

}

static int evaluate_reentrant(DiceAST *node, DiceAST *root, int recurse) {
	int res,
		rolls = 0;

	if (recurse > DICE_MAX_RECURSE)
		return DICE_ERROR_MAXRECURSE;

	switch (node->type) {
	case DICE_NODE_MATH:
		res = evaluate_reentrant(((DiceMathNode *)node)->left, root, recurse + 1);
		if (res < 0)
			return res;
		rolls = res;
		if (rolls > DICE_MAX_DICE)
			return DICE_ERROR_MAXDICE;

		res = evaluate_reentrant(((DiceMathNode *)node)->right, root, recurse + 1);
		if (res < 0)
			return res;
		rolls += res;
		if (rolls > DICE_MAX_DICE)
			return DICE_ERROR_MAXDICE;

		switch (((DiceMathNode *)node)->op) {
		case DICE_OP_ADD:
			node->value = ((DiceMathNode *)node)->left->value + ((DiceMathNode *)node)->right->value;
			break;
		case DICE_OP_SUBTRACT:
			node->value = ((DiceMathNode *)node)->left->value - ((DiceMathNode *)node)->right->value;
			break;
		case DICE_OP_MULTIPLY:
			node->value = ((DiceMathNode *)node)->left->value * ((DiceMathNode *)node)->right->value;
			break;
		case DICE_OP_DIVIDE:
			if (((DiceMathNode *)node)->right->value == 0)
				return DICE_ERROR_DIVZERO;
			node->value = ((DiceMathNode *)node)->left->value / ((DiceMathNode *)node)->right->value;
			break;
		}

		break;
	case DICE_NODE_GROUP:
		break;
	case DICE_NODE_ROLL:
		res = evaluate_reentrant(((DiceRollNode *)node)->num, root, recurse + 1);
		if (res < 0)
			return res;
		rolls = res;
		if (rolls > DICE_MAX_DICE)
			return DICE_ERROR_MAXDICE;


		break;
	case DICE_NODE_REROLL:
		break;
	case DICE_NODE_EXPLODE:
		break;
	case DICE_NODE_KEEP:
		break;
	case DICE_NODE_SUCCESS:
		break;
	case DICE_NODE_LITERAL:
		// no-op, these get evaluated directly but don't need to do anything as they already have the value set
		break;
	default:
		// encountered a node type that should not be directly evaluated
		assert(0);
		break;
	}

	if (node == root)
		return 0;

	return rolls;
}

// evaluates the tree with node as the root
// returns 0 on success (node->value contains roll value), any other number on failure
// which maps to one of the DICE_ERROR_* constants
int evaluate(DiceAST *node) {
	return evaluate_reentrant(node, node, 0);
}
