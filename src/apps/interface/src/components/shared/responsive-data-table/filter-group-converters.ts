import {
  availableFields,
  isSymbolOperator,
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

function formatSearchTerm(term: string): string {
  return /\s/.test(term) ? `"${term}"` : term;
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

    if (isSearchGroup(item)) {
      return formatSearchTerm(item.search);
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

type ParseResult<T> = { node: T; newIndex: number } | { error: string };

type ParseContext = {
  columns: ExtendedColumnDef<any>[];
  searchColumns: ExtendedColumnDef<any>[];
};

const LOGIC_KEYWORDS = ["and", "or"];

function normalizeQuery(query: string): string {
  return query.trim().replace(/≥/g, ">=").replace(/≤/g, "<=");
}

function filterableColumns(
  columns: ExtendedColumnDef<any>[],
): ExtendedColumnDef<any>[] {
  return columns.filter((c) => c.filterConfig);
}

export function searchableColumns(
  columns: ExtendedColumnDef<any>[],
): ExtendedColumnDef<any>[] {
  const flagged = columns.filter((c) => c.filterConfig?.searchable);
  if (flagged.length > 0) return flagged;
  return columns.filter(
    (c) =>
      c.filterConfig?.type === "string" &&
      c.filterConfig.operators.includes("contains"),
  );
}

export function createSearchGroup(
  term: string,
  searchColumns: ExtendedColumnDef<any>[],
): FilterGroup {
  return {
    logic: "or",
    search: term,
    items: searchColumns.map((c) => ({
      field: c.id,
      operator: "contains",
      value: term,
    })),
  };
}

export function isSearchGroup(item: FilterGroupItem): item is FilterGroup & { search: string } {
  return (
    isFilterGroup(item) &&
    item.search !== undefined &&
    item.logic === "or" &&
    item.items.length > 0 &&
    item.items.every(
      (child) =>
        isFilterCondition(child) &&
        child.operator === "contains" &&
        child.value === item.search,
    )
  );
}

function isLogicKeyword(token: string): boolean {
  return LOGIC_KEYWORDS.includes(token.toLowerCase());
}

function startsComparison(
  tokens: string[],
  index: number,
  ctx: ParseContext,
): boolean {
  const field = resolveFieldTokens(tokens, index, ctx.columns);
  return field !== undefined && matchOperator(tokens, field.newIndex) !== null;
}

function parseComparisonNode(
  tokens: string[],
  index: number,
  ctx: ParseContext,
): ParseResult<FilterCondition> {
  const resolvedField = resolveFieldTokens(tokens, index, ctx.columns);
  if (resolvedField === undefined) {
    return {
      error: `Unknown field '${tokens[index]}'. Available: ${availableFields(ctx.columns)}`,
    };
  }
  const field = resolvedField.field;
  let currentIndex = resolvedField.newIndex;

  const operatorMatch = matchOperator(tokens, currentIndex);
  if (operatorMatch === null) {
    return { error: `Expected operator after field '${field}'` };
  }
  currentIndex = operatorMatch.newIndex;

  if (currentIndex >= tokens.length || tokens[currentIndex] === ")") {
    const operatorToken = tokens[operatorMatch.newIndex - 1];
    return { error: `Expected value after '${operatorToken}'` };
  }

  const parsed = parseValue(tokens[currentIndex]);
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

function parseSearchNode(
  tokens: string[],
  index: number,
  ctx: ParseContext,
): ParseResult<FilterGroup> {
  const words: string[] = [];
  let currentIndex = index;

  while (
    currentIndex < tokens.length &&
    tokens[currentIndex] !== "(" &&
    tokens[currentIndex] !== ")" &&
    !isLogicKeyword(tokens[currentIndex]) &&
    !isSymbolOperator(tokens[currentIndex]) &&
    !startsComparison(tokens, currentIndex, ctx)
  ) {
    if (isSymbolOperator(tokens[currentIndex + 1])) {
      return {
        error: `Unknown field '${tokens[currentIndex]}'. Available: ${availableFields(ctx.columns)}`,
      };
    }
    words.push(parseValue(tokens[currentIndex]).value ?? tokens[currentIndex]);
    currentIndex++;
  }

  if (words.length === 0) {
    return { error: "Expected expression" };
  }

  if (ctx.searchColumns.length === 0) {
    return { error: "This table has no searchable text columns" };
  }

  return {
    node: createSearchGroup(words.join(" "), ctx.searchColumns),
    newIndex: currentIndex,
  };
}

function parseAtomNode(
  tokens: string[],
  index: number,
  ctx: ParseContext,
): ParseResult<FilterGroupItem> {
  if (index >= tokens.length) {
    return { error: "Expected expression" };
  }

  if (tokens[index] === "(") {
    const inner = parseOrNode(tokens, index + 1, ctx);
    if ("error" in inner) return inner;
    if (inner.newIndex >= tokens.length || tokens[inner.newIndex] !== ")") {
      return { error: "Unclosed parenthesis" };
    }
    return { node: inner.node, newIndex: inner.newIndex + 1 };
  }

  if (startsComparison(tokens, index, ctx)) {
    return parseComparisonNode(tokens, index, ctx);
  }

  if (isSymbolOperator(tokens[index])) {
    return {
      error: `Expected a field name before operator '${tokens[index]}'. Try 'field ${tokens[index]} value'.`,
    };
  }

  return parseSearchNode(tokens, index, ctx);
}

function parseAndNode(
  tokens: string[],
  index: number,
  ctx: ParseContext,
): ParseResult<FilterGroupItem> {
  const first = parseAtomNode(tokens, index, ctx);
  if ("error" in first) return first;
  const items: FilterGroupItem[] = [first.node];
  let currentIndex = first.newIndex;

  while (
    currentIndex < tokens.length &&
    tokens[currentIndex] !== ")" &&
    tokens[currentIndex].toLowerCase() !== "or"
  ) {
    if (tokens[currentIndex].toLowerCase() === "and") {
      currentIndex++;
    }
    const next = parseAtomNode(tokens, currentIndex, ctx);
    if ("error" in next) return next;
    items.push(next.node);
    currentIndex = next.newIndex;
  }

  if (items.length === 1) {
    return { node: items[0], newIndex: currentIndex };
  }
  return { node: { logic: "and", items }, newIndex: currentIndex };
}

function parseOrNode(
  tokens: string[],
  index: number,
  ctx: ParseContext,
): ParseResult<FilterGroupItem> {
  const first = parseAndNode(tokens, index, ctx);
  if ("error" in first) return first;
  const items: FilterGroupItem[] = [first.node];
  let currentIndex = first.newIndex;

  while (
    currentIndex < tokens.length &&
    tokens[currentIndex].toLowerCase() === "or"
  ) {
    const next = parseAndNode(tokens, currentIndex + 1, ctx);
    if ("error" in next) return next;
    items.push(next.node);
    currentIndex = next.newIndex;
  }

  if (items.length === 1) {
    return { node: items[0], newIndex: currentIndex };
  }
  return { node: { logic: "or", items }, newIndex: currentIndex };
}

export type QueryParseResult =
  | { group: FilterGroup }
  | { error: string };

export function parseQuery(
  query: string,
  columns: ExtendedColumnDef<any>[],
): QueryParseResult {
  const normalized = normalizeQuery(query);
  if (normalized.length === 0) {
    return { group: { logic: "and", items: [] } };
  }

  const tokens = tokenize(normalized);
  if ("error" in tokens) {
    return tokens;
  }

  const ctx: ParseContext = {
    columns: filterableColumns(columns),
    searchColumns: searchableColumns(columns),
  };
  const parseResult = parseOrNode(tokens, 0, ctx);
  if ("error" in parseResult) {
    return parseResult;
  }

  if (parseResult.newIndex < tokens.length) {
    return { error: `Unexpected '${tokens[parseResult.newIndex]}'` };
  }

  const node = parseResult.node;
  if (!isFilterGroup(node) || node.logic === "or") {
    return { group: { logic: "and", items: [node] } };
  }

  return { group: node };
}

export function queryToFilterGroup(
  query: string,
  columns: ExtendedColumnDef<any>[],
): FilterGroup | null {
  const result = parseQuery(query, columns);
  return "error" in result ? null : result.group;
}

export function removeFilterItem(
  group: FilterGroup,
  path: number[],
): FilterGroup {
  const [index, ...rest] = path;
  if (rest.length === 0) {
    return { ...group, items: group.items.filter((_, i) => i !== index) };
  }

  const items = group.items.flatMap((item, i) => {
    if (i !== index || isFilterCondition(item)) return [item];
    const updated = removeFilterItem(item, rest);
    if (updated.items.length === 0) return [];
    if (updated.items.length === 1 && updated.search === undefined) {
      return [updated.items[0]];
    }
    return [updated];
  });
  return { ...group, items };
}
