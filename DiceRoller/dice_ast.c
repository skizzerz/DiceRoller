#include <stdlib.h>
#include <assert.h>
#include "dice.h"
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

	return node;
}

DiceAST *dice_basic_node(DiceAST *num, DiceAST *sides, DiceAST *extras)
{
	DiceAST *ret;
	DiceExtras *ext;
	DiceRollNode *node;

	node = (DiceRollNode *)malloc(sizeof(DiceRollNode));
	node->base.type = DICE_NODE_ROLL;
	node->base.size = sizeof(DiceRollNode);
	node->base.value = 0;
	node->type = DICE_ROLL_NORMAL;
	node->valuesize = 0;
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

DiceAST *dice_fate_node(DiceAST *num)
{
	DiceRollNode *node;

	node = (DiceRollNode *)malloc(sizeof(DiceRollNode));
	node->base.type = DICE_NODE_ROLL;
	node->base.size = sizeof(DiceRollNode);
	node->base.value = 0;
	node->type = DICE_ROLL_FATE;
	node->valuesize = 0;
	node->num = num;
	node->sides = NULL;
	node->values = NULL;

	return (DiceAST *)node;
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
	node->rolled = 0;

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
	node->rolled = 0;

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
	// FIXME: replace with an RNG that guarantees an even distribution
	return (rand() % max) + 1;
}

// performs a comparison, returning 1 if the comparison is true and 0 if false
static int dicecmp(int type, float dice_value, float comp_value) {
	switch (type) {
	case DICE_COMP_EQUAL:
		return dice_value == comp_value;
	case DICE_COMP_LESSER:
		return dice_value < comp_value;
	case DICE_COMP_GREATER:
		return dice_value > comp_value;
	}

	// getting here means the comparison was ill-formed, which is an error
	assert(0);
	return 0;
}

// find an underlying "roll" of this node, which can be a roll, grouped roll, or keep node
// (e.g. things with a "values" array that holds individual results)
DiceAST *find_roll(DiceAST *node) {
	switch (node->type) {
	case DICE_NODE_ROLL:
	case DICE_NODE_GROUP:
	case DICE_NODE_KEEP:
		return node;
	case DICE_NODE_REROLL:
		return find_roll(((DiceRerollNode *)node)->expr);
	case DICE_NODE_EXPLODE:
		return find_roll(((DiceExplodeNode *)node)->expr);
	case DICE_NODE_SUCCESS:
		return find_roll(((DiceSuccessNode *)node)->expr);
	case DICE_NODE_MATH:
	{
		// for math nodes, we only return a value if one but not both of the children have a roll
		// if both have a roll, return NULL since not sure how to handle this yet, an API to get at multiple
		// sets of rolls may be required for this
		DiceAST *left = find_roll(((DiceMathNode *)node)->left);
		DiceAST *right = find_roll(((DiceMathNode *)node)->right);

		if (left != NULL && right != NULL)
			return NULL;

		return (left == NULL) ? right : left;
	}
	default:
		// getting the underlying dice rolled is not supported for this node
		return NULL;
	}
}

static int intcmp(const void *p1, const void *p2) {
	int x = *(const int *)p1;
	int y = *(const int *)p2;

	if (x == y)
		return 0;
	if (x < y)
		return -1;
	return 1;
}

static int evaluate_reentrant(DiceAST *node, DiceAST *root, int recurse) {
	int res, i, rolls = 0;

	if (recurse > DICE_MAX_RECURSE)
		return DICE_ERROR_MAXRECURSE;

	switch (node->type) {
	case DICE_NODE_MATH:
	{
		DiceMathNode *self = (DiceMathNode *)node;

		res = evaluate_reentrant(self->left, root, recurse + 1);
		if (res < 0)
			return res;
		rolls = res;

		res = evaluate_reentrant(self->right, root, recurse + 1);
		if (res < 0)
			return res;
		rolls += res;
		if (rolls > DICE_MAX_DICE)
			return DICE_ERROR_MAXDICE;

		switch (self->op) {
		case DICE_OP_ADD:
			node->value = self->left->value + self->right->value;
			break;
		case DICE_OP_SUBTRACT:
			node->value = self->left->value - self->right->value;
			break;
		case DICE_OP_MULTIPLY:
			node->value = self->left->value * self->right->value;
			break;
		case DICE_OP_DIVIDE:
			if (self->right->value == 0)
				return DICE_ERROR_DIVZERO;
			node->value = self->left->value / self->right->value;
			break;
		}

		break;
	}
	case DICE_NODE_GROUP:
	{
		DiceGroupedRollNode *self = (DiceGroupedRollNode *)node;
		int run;
		short num;

		res = evaluate_reentrant(self->num, root, recurse + 1);
		if (res < 0)
			return res;
		rolls = res;

		num = (short)self->num->value;
		self->values = (int *)malloc(num * self->groupsize * sizeof(int));
		self->valuesize = num * self->groupsize;
		node->value = 0;

		for (run = 0; run < num; ++run) {
			int offset = num * self->groupsize;

			for (i = 0; i < self->groupsize; ++i) {
				res = evaluate_reentrant(self->exprs[i], root, recurse + 1);
				if (res < 0)
					return res;
				rolls += res;
				if (rolls > DICE_MAX_DICE)
					return DICE_ERROR_MAXDICE;

				self->values[offset + i] = (int)self->exprs[i]->value;
				node->value += self->values[offset + i];
			}
		}

		break;
	}
	case DICE_NODE_ROLL:
	{
		DiceRollNode *self = (DiceRollNode *)node;
		int num, val, sides, sub = 0;

		res = evaluate_reentrant(self->num, root, recurse + 1);
		if (res < 0)
			return res;
		rolls = res;

		num = (int)self->num->value;
		if (num < 1)
			return DICE_ERROR_MINDICE;
		rolls += num;
		if (rolls > DICE_MAX_DICE)
			return DICE_ERROR_MAXDICE;

		if (self->type == DICE_ROLL_NORMAL) {
			res = evaluate_reentrant(self->sides, root, recurse + 1);
			if (res < 0)
				return res;
			rolls += res;
			sides = (int)self->sides->value;
			if (sides > DICE_MAX_SIDES)
				return DICE_ERROR_MAXSIDES;
			if (sides < 1)
				return DICE_ERROR_MINSIDES;
		} else if (self->type == DICE_ROLL_FATE) {
			sides = 3;
			// normal range is [1, sides], for fate we want [-1, 1] so we subtract 2 from the result
			sub = 2;
		} else {
			// invalid type
			assert(0);
		}

		self->values = (int *)malloc(num * sizeof(int));
		self->valuesize = (short)num;
		node->value = 0;

		for (i = 0; i < num; ++i) {
			val = rand_int(sides) - sub;
			self->values[i] = val;
			node->value += val;
		}

		break;
	}
	case DICE_NODE_REROLL:
	{
		DiceRerollNode *self = (DiceRerollNode *)node;
		DiceCompareNode *cond = (DiceCompareNode *)self->cond;
		// self->expr is guaranteed to be a DiceRollNode based on our grammar
		DiceRollNode *roll = (DiceRollNode *)self->expr;
		int num, val, sides, sub = 0;

		res = evaluate_reentrant(self->expr, root, recurse + 1);
		if (res < 0)
			return res;
		rolls = res;

		res = evaluate_reentrant(cond->expr, root, recurse + 1);
		if (res < 0)
			return res;
		rolls += res;
		if (rolls > DICE_MAX_DICE)
			return DICE_ERROR_MAXDICE;

		++self->rolled;

		num = (int)roll->num->value;

		if (roll->type == DICE_ROLL_NORMAL) {
			sides = (int)roll->sides->value;
		} else {
			sides = 3;
			sub = 2;
		}

		// perform the rerolls here without reevaluating the subtree
		// (which makes the number of dice and number of sides essentially fixed values)
		while (dicecmp(cond->type, roll->base.value, cond->base.value)) {
			rolls += num;
			if (rolls > DICE_MAX_DICE)
				return DICE_ERROR_MAXDICE;

			roll->base.value = 0;
			for (i = 0; i < num; ++i) {
				val = rand_int(sides) - sub;
				roll->values[i] = val;
				roll->base.value += val;
			}

			if (self->once && self->rolled > 1)
				break;
		}

		node->value = roll->base.value;

		break;
	}
	case DICE_NODE_EXPLODE:
	{
		DiceExplodeNode *self = (DiceExplodeNode *)node;
		DiceCompareNode *cond = (DiceCompareNode *)self->cond;
		DiceRollNode *roll = find_roll(self->expr);
		// fate dice cannot be exploded
		assert(roll->type == DICE_ROLL_NORMAL);
		int num = (int)roll->num->value;
		int sides = (int)roll->sides->value;
		int cond_type, val;
		float cond_comp;

		res = evaluate_reentrant(self->expr, root, recurse + 1);
		if (res < 0)
			return res;
		rolls = res;

		if (cond != NULL) {
			res = evaluate_reentrant(cond->expr, root, recurse + 1);
			if (res < 0)
				return res;
			rolls += res;
			if (rolls > DICE_MAX_DICE)
				return DICE_ERROR_MAXDICE;
			cond_type = cond->type;
			cond_comp = cond->expr->value;
		} else {
			cond_type = DICE_COMP_EQUAL;
			cond_comp = sides;
		}

		// go through our roll values and explode all of them that match cond
		// if cond is NULL then we explode all of them with a result equal to the number of sides
		switch (self->type) {
		case DICE_EXPLODE_EXPLODE:
			// new dice are added to the end, existing values are unchanged
			for (i = 0; i < num; ++i) {
				if (dicecmp(cond_type, roll->values[i], cond_comp)) {
					++rolls;
					if (rolls > DICE_MAX_DICE)
						return DICE_ERROR_MAXDICE;

					roll->values = (int *)realloc(roll->values, (num + 1) * sizeof(int));
					val = rand_int(sides);
					roll->values[num] = val;
					roll->base.value += val;
					++roll->valuesize;
					++num;
				}
			}
			break;
		case DICE_EXPLODE_COMPOUND:
			// new dice are added to the existing values, leaving number of individual die values unchanged
			for (i = 0; i < num; ++i) {
				val = roll->values[i];

				while (dicecmp(cond_type, val, cond_comp)) {
					++rolls;
					if (rolls > DICE_MAX_DICE)
						return DICE_ERROR_MAXDICE;

					val = rand_int(sides);
					roll->values[i] += val;
					roll->base.value += val;
				}
			}
			break;
		case DICE_EXPLODE_PENETRATE:
		{
			// like DICE_EXPLODE_COMPOUND, except 1 is subtracted from each additional roll, and
			// d100s downgrade to d20s, and d20s downgrade to d6s (per HackMaster 5e rules).
			// Downgraded d20s do not further downgrade, however. Downgrading is not performed
			// if an explicit condition is specified (e.g. cond != NULL), which allows for
			// HackMaster 4e style penetrating dice which do not downgrade.
			int orig_comp = cond_comp, orig_sides = sides, downgraded;

			for (i = 0; i < num; ++i) {
				val = roll->values[i];
				downgraded = 0;
				sides = orig_sides;
				cond_comp = orig_comp;

				while (dicecmp(cond_type, val, cond_comp)) {
					++rolls;
					if (rolls > DICE_MAX_DICE)
						return DICE_ERROR_MAXDICE;

					if (downgraded == 0 && cond == NULL) {
						if (sides == 100) {
							downgraded = 1;
							sides = 20;
							cond_comp = 20;
						} else if (sides == 20) {
							downgraded = 1;
							sides = 6;
							cond_comp = 6;
						}
					}

					val = rand_int(sides);
					roll->values[i] += val - 1;
					roll->base.value += val - 1;
				}
			}
			break;
		}
		}

		node->value = roll->base.value;

		break;
	}
	case DICE_NODE_KEEP:
	{
		DiceKeepNode *self = (DiceKeepNode *)node;
		DiceAST *rollNode = find_roll(self->expr);

		res = evaluate_reentrant(self->expr, root, recurse + 1);
		if (res < 0)
			return res;
		rolls = res;

		res = evaluate_reentrant(self->amount, root, recurse + 1);
		if (res < 0)
			return res;
		rolls += res;
		if (rolls > DICE_MAX_DICE)
			return DICE_ERROR_MAXDICE;

		if (rollNode->type == DICE_NODE_ROLL) {
			DiceRollNode *roll = (DiceRollNode *)rollNode;
			// can't use a keep node on fate dice
			assert(roll->type == DICE_ROLL_NORMAL);

			// sort die values lowest->highest so we can re-use the same underlying array
			qsort(roll->values, roll->valuesize, sizeof(int), intcmp);

			// determine our offset into roll->values depending on keep type
			switch (self->type) {
			case DICE_KEEP_KEEP_LOW:
				self->num = min((short)self->amount->value, roll->valuesize);
				self->values = roll->values;
				break;
			case DICE_KEEP_KEEP_HIGH:
				self->num = min((short)self->amount->value, roll->valuesize);
				self->values = roll->values + roll->valuesize - self->num;
				break;
			case DICE_KEEP_DROP_LOW:
				self->num = max(roll->valuesize - (short)self->amount->value, 1);
				self->values = roll->values + roll->valuesize - self->num;
				break;
			case DICE_KEEP_DROP_HIGH:
				self->num = max(roll->valuesize - (short)self->amount->value, 1);
				self->values = roll->values;
			}
		} else if (rollNode->type == DICE_NODE_GROUP) {
			DiceGroupedRollNode *group = (DiceGroupedRollNode *)rollNode;

			// sort die values lowest->highest so we can re-use the same underlying array
			qsort(group->values, group->valuesize, sizeof(int), intcmp);

			// determine our offset into group->values depending on keep type
			switch (self->type) {
			case DICE_KEEP_KEEP_LOW:
				self->num = min((short)self->amount->value, group->valuesize);
				self->values = group->values;
				break;
			case DICE_KEEP_KEEP_HIGH:
				self->num = min((short)self->amount->value, group->valuesize);
				self->values = group->values + group->valuesize - self->num;
				break;
			case DICE_KEEP_DROP_LOW:
				self->num = max(group->valuesize - (short)self->amount->value, 1);
				self->values = group->values + group->valuesize - self->num;
				break;
			case DICE_KEEP_DROP_HIGH:
				self->num = max(group->valuesize - (short)self->amount->value, 1);
				self->values = group->values;
			}
		} else {
			// trying to keep a keep node, or something; not allowed by grammar regardless
			assert(0);
		}

		node->value = 0;
		for (i = 0; i < self->num; ++i) {
			node->value += self->values[i];
		}

		break;
	}
	case DICE_NODE_SUCCESS:
	{
		DiceSuccessNode *self = (DiceSuccessNode *)node;
		DiceCompareNode *success = (DiceCompareNode *)self->success_cond;
		DiceCompareNode *failure = (DiceCompareNode *)self->failure_cond;
		DiceAST *rollNode = find_roll(self->expr);
		int *values;
		int num;

		res = evaluate_reentrant(self->expr, root, recurse + 1);
		if (res < 0)
			return res;
		rolls = res;

		res = evaluate_reentrant(self->success_cond, root, recurse + 1);
		if (res < 0)
			return res;
		rolls += res;
		if (rolls > DICE_MAX_DICE)
			return DICE_ERROR_MAXDICE;

		if (failure != NULL) {
			res = evaluate_reentrant(self->failure_cond, root, recurse + 1);
			if (res < 0)
				return res;
			rolls += res;
			if (rolls > DICE_MAX_DICE)
				return DICE_ERROR_MAXDICE;
		}

		switch (rollNode->type) {
		case DICE_NODE_ROLL:
			values = ((DiceRollNode *)rollNode)->values;
			num = ((DiceRollNode *)rollNode)->valuesize;
			break;
		case DICE_NODE_GROUP:
			values = ((DiceGroupedRollNode *)rollNode)->values;
			num = ((DiceGroupedRollNode *)rollNode)->valuesize;
			break;
		case DICE_NODE_KEEP:
			values = ((DiceKeepNode *)rollNode)->values;
			num = ((DiceKeepNode *)rollNode)->num;
			break;
		default:
			// invalid node type
			assert(0);
			break;
		}

		self->successes = 0;
		self->failures = 0;
		for (i = 0; i < num; ++i) {
			if (dicecmp(success->type, values[i], success->base.value)) {
				++self->successes;
			} else if (failure != NULL && dicecmp(failure->type, values[i], failure->base.value)) {
				++self->failures;
			}
		}

		node->value = self->successes - self->failures;

		break;
	}
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

// recursively frees an AST tree
void free_tree(DiceAST *root) {
	int i;

	if (root == NULL)
		return;

	switch (root->type) {
	case DICE_NODE_COMPARE:
		free_tree(((DiceCompareNode *)root)->expr);
		break;
	case DICE_NODE_EXPLODE:
		free_tree(((DiceExplodeNode *)root)->cond);
		free_tree(((DiceExplodeNode *)root)->expr);
		break;
	case DICE_NODE_EXTRAS:
		free_tree(((DiceExtras *)root)->explode);
		free_tree(((DiceExtras *)root)->keep);
		free_tree(((DiceExtras *)root)->reroll);
		free_tree(((DiceExtras *)root)->success);
		break;
	case DICE_NODE_GROUP:
		for (i = 0; i < ((DiceGroupedRollNode *)root)->groupsize; ++i) {
			free_tree(((DiceGroupedRollNode *)root)->exprs[i]);
		}
		free_tree(((DiceGroupedRollNode *)root)->num);
		free(((DiceGroupedRollNode *)root)->exprs);
		free(((DiceGroupedRollNode *)root)->values);
		break;
	case DICE_NODE_KEEP:
		free_tree(((DiceKeepNode *)root)->amount);
		free_tree(((DiceKeepNode *)root)->expr);
		// do not free values here, as it wasn't allocated by us; it belongs to a node further down the tree
		break;
	case DICE_NODE_LITERAL:
		// nothing to free here
		break;
	case DICE_NODE_MATH:
		free_tree(((DiceMathNode *)root)->left);
		free_tree(((DiceMathNode *)root)->right);
		break;
	case DICE_NODE_NULL:
		// nothing to free here
		break;
	case DICE_NODE_REROLL:
		free_tree(((DiceRerollNode *)root)->cond);
		free_tree(((DiceRerollNode *)root)->expr);
		break;
	case DICE_NODE_ROLL:
		free_tree(((DiceRollNode *)root)->num);
		free_tree(((DiceRollNode *)root)->sides);
		free(((DiceRollNode *)root)->values);
		break;
	case DICE_NODE_SUCCESS:
		free_tree(((DiceSuccessNode *)root)->expr);
		free_tree(((DiceSuccessNode *)root)->failure_cond);
		free_tree(((DiceSuccessNode *)root)->success_cond);
		break;
	default:
		// invalid node type
		assert(0);
		break;
	}

	// before freeing us, wipe out the type/size info so that the node is no longer considered valid
	// in case someone passes us a freed pointer to a previously valid node
	root->type = DICE_NODE_NULL;
	root->size = 0;
	free(root);
}
