// for MSVC, because it's dumb
#define YY_NO_UNISTD_H
#define YY_USE_CONST

#include "dice.h"
#include "dice_ast.h"
#include "dice_parser.h"
#include "dice_lexer.h"
#include <stdlib.h>

// sample main function for debugging purposes
#ifdef DICE_GEN_MAIN
#include <stdio.h>

int main() {
	void *result;
	int res;
	float total;

	res = dice_evaluate("3d6", &result);
	if (res != 0) {
		printf("Error: %d\n", res);
		return 0;
	}

	res = dice_total(result, &total);
	if (res != 0) {
		printf("Error: %d\n", res);
		return 0;
	}

	printf("Total: %f\n", total);

	return 0;
}
#endif

// ensures a node is valid, e.g. that it's size and type match up properly
// this is done as a sanity check to ensure we didn't receive bogus input
static int validate_node(DiceAST *node) {
	switch (node->type) {
	case DICE_NODE_EXPLODE:
		return node->size == sizeof(DiceExplodeNode);
	case DICE_NODE_GROUP:
		return node->size == sizeof(DiceGroupedRollNode);
	case DICE_NODE_KEEP:
		return node->size == sizeof(DiceKeepNode);
	case DICE_NODE_LITERAL:
		return node->size == sizeof(DiceLiteralNode);
	case DICE_NODE_MATH:
		return node->size == sizeof(DiceMathNode);
	case DICE_NODE_REROLL:
		return node->size == sizeof(DiceRerollNode);
	case DICE_NODE_ROLL:
		return node->size == sizeof(DiceRollNode);
	case DICE_NODE_SUCCESS:
		return node->size == sizeof(DiceSuccessNode);
	default:
		// invalid node type or a node that should never be the root of an AST
		// (e.g DICE_NODE_EXTRAS, DICE_NODE_NULL, DICE_NODE_COMPARE)
		return 0;
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

static void extract_results(DiceAST *rollNode, int **results, int *num_results) {
	switch (rollNode->type) {
	case DICE_NODE_ROLL:
		*results = ((DiceRollNode *)rollNode)->values;
		*num_results = (int)((DiceRollNode *)rollNode)->valuesize;
		break;
	case DICE_NODE_GROUP:
		*results = ((DiceGroupedRollNode *)rollNode)->values;
		*num_results = ((DiceGroupedRollNode *)rollNode)->valuesize;
		break;
	case DICE_NODE_KEEP:
		*results = ((DiceKeepNode *)rollNode)->values;
		*num_results = ((DiceKeepNode *)rollNode)->num;
		break;
	}

	// sort our results for output
	qsort(*results, *num_results, sizeof(int), intcmp);
}

int dice_evaluate(const char *input, void **dice_result) {
	DiceAST *result;
	yyscan_t scanner;
	YY_BUFFER_STATE state;
	int res;

	*dice_result = NULL;

	if (yylex_init(&scanner)) {
		// could not init
		return DICE_ERROR_NOFLEX;
	}

	state = yy_scan_string(input, scanner);

	if (yyparse(&result, scanner)) {
		yy_delete_buffer(state, scanner);
		yylex_destroy(scanner);
		return DICE_ERROR_SYNTAX;
	}

	yy_delete_buffer(state, scanner);
	yylex_destroy(scanner);

	res = evaluate(result);

	if (res < 0) {
		free_tree(result);
		return res;
	}

	*dice_result = (void *)result;
	return 0;
}

int dice_total(void *dice_result, float *total) {
	*total = 0;

	if (dice_result == NULL)
		return DICE_ERROR_NULL_RESULT;

	DiceAST *result = (DiceAST *)dice_result;
	if (!validate_node(result))
		return DICE_ERROR_INVALID_RESULT;

	*total = result->value;
	return 0;
}

int dice_results(void *dice_result, int **results, int *num_results) {
	*results = NULL;
	*num_results = 0;

	if (dice_result == NULL)
		return DICE_ERROR_NULL_RESULT;

	DiceAST *result = (DiceAST *)dice_result;
	if (!validate_node(result))
		return DICE_ERROR_INVALID_RESULT;

	DiceAST *roll = find_roll(result);
	if (roll == NULL)
		return DICE_ERROR_NODICE;

	extract_results(roll, results, num_results);
	return 0;
}

int dice_raw_results(void *dice_result, int **results, int *num_results) {
	*results = NULL;
	*num_results = 0;

	if (dice_result == NULL)
		return DICE_ERROR_NULL_RESULT;

	DiceAST *result = (DiceAST *)dice_result;
	if (!validate_node(result))
		return DICE_ERROR_INVALID_RESULT;

	DiceAST *roll = find_roll(result);
	if (roll == NULL)
		return DICE_ERROR_NODICE;

	// if we're given a keep node, get the one under it
	if (roll->type == DICE_NODE_KEEP) {
		roll = find_roll(((DiceKeepNode *)roll)->expr);
		if (roll == NULL)
			return DICE_ERROR_NODICE;
	}

	extract_results(roll, results, num_results);
	return 0;
}

void dice_free(void *dice_result) {
	if (dice_result == NULL)
		return;

	DiceAST *result = (DiceAST *)dice_result;
	if (!validate_node(result))
		return;

	free_tree(result);
}
