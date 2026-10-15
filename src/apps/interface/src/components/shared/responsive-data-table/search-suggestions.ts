import { OPERATOR_DISPLAY } from "./filter-group-converters";
import { matchOperator, resolveFieldTokens, tokenize } from "./filter-parser-utils";
import { type ExtendedColumnDef } from "./types";

export type SearchSuggestion = {
  label: string;
  query: string;
};

const MAX_SUGGESTIONS = 6;
const MIN_PREFIX_LENGTH = 2;
const MAX_FIELD_WORDS = 3;

function columnLabel(column: ExtendedColumnDef<any>): string {
  return typeof column.header === "string" ? column.header : column.id;
}

function quoteIfNeeded(value: string): string {
  return /\s/.test(value) ? `"${value}"` : value;
}

function followsOperator(tokens: string[], index: number): boolean {
  for (let start = Math.max(0, index - 2); start < index; start++) {
    if (matchOperator(tokens, start)?.newIndex === index) return true;
  }
  return false;
}

function fieldSuggestions(
  query: string,
  tokens: string[],
  columns: ExtendedColumnDef<any>[],
): SearchSuggestion[] {
  for (let count = Math.min(MAX_FIELD_WORDS, tokens.length); count >= 1; count--) {
    const start = tokens.length - count;
    const partial = tokens.slice(start).join(" ");
    if (partial.length < MIN_PREFIX_LENGTH || followsOperator(tokens, start)) {
      continue;
    }

    const lower = partial.toLowerCase();
    const matches = columns.filter((c) => {
      const label = columnLabel(c).toLowerCase();
      return label.startsWith(lower) && label !== lower;
    });
    if (matches.length === 0) continue;

    const partialIndex = query.toLowerCase().lastIndexOf(lower);
    if (partialIndex < 0) continue;

    const prefix = query.slice(0, partialIndex);
    return matches.slice(0, MAX_SUGGESTIONS).map((c) => ({
      label: columnLabel(c),
      query: `${prefix}${columnLabel(c)} `,
    }));
  }
  return [];
}

function resolveTrailingField(
  tokens: string[],
  end: number,
  columns: ExtendedColumnDef<any>[],
): ExtendedColumnDef<any> | undefined {
  for (let start = Math.max(0, end - MAX_FIELD_WORDS); start < end; start++) {
    const resolved = resolveFieldTokens(tokens.slice(0, end), start, columns);
    if (resolved?.newIndex === end) {
      return columns.find((c) => c.id === resolved.field);
    }
  }
  return undefined;
}

function operatorSuggestions(
  query: string,
  column: ExtendedColumnDef<any>,
): SearchSuggestion[] {
  return (column.filterConfig?.operators ?? []).map((operator) => ({
    label: OPERATOR_DISPLAY[operator].natural,
    query: `${query}${OPERATOR_DISPLAY[operator].natural} `,
  }));
}

function valueSuggestions(
  query: string,
  tokens: string[],
  columns: ExtendedColumnDef<any>[],
): SearchSuggestion[] {
  for (let start = tokens.length - 1; start >= Math.max(0, tokens.length - 2); start--) {
    const operator = matchOperator(tokens, start);
    if (operator?.newIndex !== tokens.length) continue;

    const column = resolveTrailingField(tokens, start, columns);
    const config = column?.filterConfig;
    const values =
      config?.type === "boolean" ? ["true", "false"] : (config?.enumValues ?? []);
    return values.slice(0, MAX_SUGGESTIONS * 2).map((value) => ({
      label: value,
      query: `${query}${quoteIfNeeded(value)} `,
    }));
  }
  return [];
}

export function getSearchSuggestions(
  query: string,
  columns: ExtendedColumnDef<any>[],
): SearchSuggestion[] {
  const tokens = tokenize(query);
  if ("error" in tokens || tokens.length === 0) return [];

  const filterable = columns.filter((c) => c.filterConfig);

  if (!/\s$/.test(query)) {
    return fieldSuggestions(query, tokens, filterable);
  }

  const values = valueSuggestions(query, tokens, filterable);
  if (values.length > 0) return values;

  const column = resolveTrailingField(tokens, tokens.length, filterable);
  return column ? operatorSuggestions(query, column) : [];
}
