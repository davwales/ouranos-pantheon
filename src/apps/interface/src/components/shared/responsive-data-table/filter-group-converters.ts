import {
  availableFields,
  matchOperator,
  parseValue,
  resolveFieldTokens,
  tokenize,
} from "./filter-parser-utils";
import {
  BACKEND_OPERATOR,
  type ExtendedColumnDef,
  type FilterCondition,
  type FilterGroup,
  type FilterGroupItem,
  type FilterOperator,
  isFilterCondition,
  isFilterGroup,
} from "./types";

export const OPERATOR_DISPLAY: Record<FilterOperator, { natural: string; glyph: string }> = {
  eq:        { natural: "is",       glyph: "=" },
  neq:       { natural: "is not",   glyph: "≠" },
  gt:        { natural: ">",        glyph: ">" },
  gte:       { natural: "≥",        glyph: "≥" },
  lt:        { natural: "<",        glyph: "<" },
  lte:       { natural: "≤",        glyph: "≤" },
  contains:  { natural: "contains",  glyph: "contains" },
  startsWith:{ natural: "starts with", glyph: "starts with" },
  endsWith:  { natural: "ends with", glyph: "ends with" },
};

const BACKEND_TO_FRONTEND_OPERATOR: Record<string, FilterOperator> =
  Object.fromEntries(
    Object.entries(BACKEND_OPERATOR).map(([fe, be]) => [be, fe]),
  ) as Record<string, FilterOperator>;

function formatQueryCondition(
  condition: FilterCondition,
  columns?: ExtendedColumnDef<any>[],
): string {
  const { field, operator, value } = condition;

  const col = columns?.find((c) => c.id === field);
  const fieldName =
    col && typeof col.header === "string" ? col.header : field;

  if (value === null) {
    return operator === "eq"
      ? `${fieldName} is null`
      : `${fieldName} is not null`;
  }

  const operatorLabel = OPERATOR_DISPLAY[operator].natural;
  const stringValue = String(value);

  if (/\s/.test(stringValue)) {
    return `${fieldName} ${operatorLabel} "${stringValue}"`;
  }

  return `${fieldName} ${operatorLabel} ${stringValue}`;
}

export function filterGroupToQuery(
  group: FilterGroup,
  columns?: ExtendedColumnDef<any>[],
): string {
  if (group.items.length === 0) return "";

  const serialize = (item: FilterGroupItem, depth: number): string => {
    if (isFilterCondition(item)) {
      return formatQueryCondition(item, columns);
    }

    const parts = item.items
      .map((child) => serialize(child, depth + 1))
      .filter(Boolean);
    if (parts.length === 0) return "";
    const joined =
      item.logic === "and" ? parts.join(" and ") : parts.join(" or ");

    if (depth === 0 && item.logic === "and") {
      return joined;
    }

    return `(${joined})`;
  };

  return serialize(group, 0);
}

type TokenResult = { tokens: string[] } | { error: string };
type ParseResult<T> = { node: T; newIndex: number } | { error: string };

function normalizeQuery(query: string): string {
  return query.trim().replace(/≥/g, ">=").replace(/≤/g, "<=");
}

function parseComparisonNode(
  tokens: string[],
  index: number,
  columns: ExtendedColumnDef<any>[],
): ParseResult<FilterCondition> {
  if (index >= tokens.length) {
    return { error: "Expected comparison" };
  }

  let field: string;
  let currentIndex = index;

  const resolvedField = resolveFieldTokens(tokens, currentIndex, columns);
  if (resolvedField !== undefined) {
    field = resolvedField.field;
    currentIndex = resolvedField.newIndex;
  } else if (matchOperator(tokens, currentIndex) !== null) {
    return {
      error: `Expected a field name before operator '${tokens[index]}'. Try 'field ${tokens[index]} value'.`,
    };
  } else {
    return {
      error: `Unknown field '${tokens[index]}'. Available: ${availableFields(columns)}`,
    };
  }

  const operatorMatch = matchOperator(tokens, currentIndex);
  if (operatorMatch === null) {
    const token = tokens[currentIndex];
    const message =
      token === undefined
        ? `Expected operator after field '${field}'`
        : `Expected operator after field '${field}', found '${token}'`;
    return { error: message };
  }
  currentIndex = operatorMatch.newIndex;

  if (currentIndex >= tokens.length) {
    const operatorToken = tokens[operatorMatch.newIndex - 1];
    return { error: `Expected value after '${operatorToken}'` };
  }

  const rawValueToken = tokens[currentIndex];
  const parsed = parseValue(rawValueToken);
  currentIndex++;

  const frontendOperator = BACKEND_TO_FRONTEND_OPERATOR[operatorMatch.backend];
  if (frontendOperator === undefined) {
    return { error: `Unsupported operator '${operatorMatch.backend}'` };
  }

  return {
    node: { field, operator: frontendOperator, value: parsed.value },
    newIndex: currentIndex,
  };
}

function parseAtomNode(
  tokens: string[],
  index: number,
  columns: ExtendedColumnDef<any>[],
): ParseResult<FilterGroupItem> {
  if (index >= tokens.length) {
    return { error: "Expected expression" };
  }

  if (tokens[index] === "(") {
    const inner = parseOrNode(tokens, index + 1, columns);
    if ("error" in inner) return inner;
    if (inner.newIndex >= tokens.length || tokens[inner.newIndex] !== ")") {
      return { error: "Unclosed parenthesis" };
    }
    return { node: inner.node, newIndex: inner.newIndex + 1 };
  }

  return parseComparisonNode(tokens, index, columns);
}

function parseBinary(
  tokens: string[],
  index: number,
  columns: ExtendedColumnDef<any>[],
  keyword: "and" | "or",
  parseChild: (
    tokens: string[],
    index: number,
    columns: ExtendedColumnDef<any>[],
  ) => ParseResult<FilterGroupItem>,
): ParseResult<FilterGroupItem> {
  let first = parseChild(tokens, index, columns);
  if ("error" in first) return first;
  let items: FilterGroupItem[] = [first.node];
  let currentIndex = first.newIndex;
  while (
    currentIndex < tokens.length &&
    tokens[currentIndex].toLowerCase() === keyword
  ) {
    currentIndex++;
    const next = parseChild(tokens, currentIndex, columns);
    if ("error" in next) return next;
    items.push(next.node);
    currentIndex = next.newIndex;
  }
  if (items.length === 1) {
    return { node: items[0], newIndex: currentIndex };
  }
  return { node: { logic: keyword, items }, newIndex: currentIndex };
}

function parseAndNode(
  tokens: string[],
  index: number,
  columns: ExtendedColumnDef<any>[],
): ParseResult<FilterGroupItem> {
  return parseBinary(tokens, index, columns, "and", parseAtomNode);
}

function parseOrNode(
  tokens: string[],
  index: number,
  columns: ExtendedColumnDef<any>[],
): ParseResult<FilterGroupItem> {
  return parseBinary(tokens, index, columns, "or", parseAndNode);
}

export function queryToFilterGroup(
  query: string,
  columns: ExtendedColumnDef<any>[],
): FilterGroup | null {
  const normalized = normalizeQuery(query);
  if (normalized.length === 0) {
    return { logic: "and", items: [] };
  }

  const tokenizeResult = tokenize(normalized);
  if ("error" in tokenizeResult) {
    return null;
  }

  const tokens = tokenizeResult;
  const parseResult = parseOrNode(tokens, 0, columns);
  if ("error" in parseResult) {
    return null;
  }

  if (parseResult.newIndex < tokens.length) {
    return null;
  }

  const node = parseResult.node;
  if (!isFilterGroup(node)) {
    return { logic: "and", items: [node] };
  }

  if (node.logic === "or") {
    return { logic: "and", items: [node] };
  }

  return node;
}
