import { type ExtendedColumnDef } from "./types";

export {
  tokenize,
  resolveFieldTokens,
  availableFields,
  matchOperator,
  parseValue,
};

const OPERATORS: Record<string, string[][]> = {
  eq:         [["="], ["equals"], ["equal", "to"], ["is"]],
  neq:        [["!="], ["not"], ["is", "not"]],
  gt:         [[">"], ["above"], ["greater", "than"], ["more", "than"]],
  gte:        [[">="], ["at", "least"]],
  lt:         [["<"], ["below"], ["less", "than"]],
  lte:        [["<="], ["at", "most"]],
  like:       [["contains"], ["like"]],
  in:         [["in"]],
  startswith: [["starts"], ["starts", "with"]],
  endswith:   [["ends"], ["ends", "with"]],
};

function isQuote(char: string): boolean {
  return char === '"' || char === "'";
}

function tokenize(input: string): string[] | { error: string } {
  const tokens: string[] = [];
  let i = 0;

  while (i < input.length) {
    const char = input[i];

    if (/\s/.test(char)) {
      i++;
      continue;
    }

    if (char === "(" || char === ")") {
      tokens.push(char);
      i++;
      continue;
    }

    if (isQuote(char)) {
      const quote = char;
      let j = i + 1;
      while (j < input.length && input[j] !== quote) {
        j++;
      }
      if (j >= input.length) {
        return {
          error: `Unclosed ${quote === '"' ? "double" : "single"} quote`,
        };
      }
      tokens.push(input.slice(i, j + 1));
      i = j + 1;
      continue;
    }

    let j = i + 1;
    while (
      j < input.length &&
      !/\s/.test(input[j]) &&
      input[j] !== "(" &&
      input[j] !== ")"
    ) {
      j++;
    }
    tokens.push(input.slice(i, j));
    i = j;
  }

  return tokens;
}

function resolveFieldTokens(
  tokens: string[],
  index: number,
  columns: ExtendedColumnDef<any>[],
): { field: string; newIndex: number } | undefined {
  const maxLen = columns.reduce((max, col) => {
    const label = typeof col.header === "string" ? col.header : col.id;
    return Math.max(max, label.split(/\s+/).length);
  }, 1);

  for (let len = Math.min(maxLen, tokens.length - index); len >= 1; len--) {
    const candidate = tokens.slice(index, index + len).join(" ").toLowerCase();
    const col = columns.find(
      (c) =>
        c.id.toLowerCase() === candidate ||
        (typeof c.header === "string" && c.header.toLowerCase() === candidate),
    );
    if (col) return { field: col.id, newIndex: index + len };
  }
  return undefined;
}

function availableFields(columns: ExtendedColumnDef<any>[]): string {
  return columns
    .map((col) => (typeof col.header === "string" ? col.header : col.id))
    .join(", ");
}

function matchOperator(
  tokens: string[],
  index: number,
): { backend: string; newIndex: number } | null {
  let best: { backend: string; newIndex: number } | null = null;
  for (const [backend, aliases] of Object.entries(OPERATORS)) {
    for (const alias of aliases) {
      const end = index + alias.length;
      if (end > tokens.length) continue;
      let ok = true;
      for (let i = 0; i < alias.length; i++) {
        if (tokens[index + i].toLowerCase() !== alias[i]) { ok = false; break; }
      }
      if (ok) {
        const candidate = { backend, newIndex: end };
        if (best === null || candidate.newIndex > best.newIndex) best = candidate;
      }
    }
  }
  return best;
}

function parseValue(token: string): { value: string | null; raw: string } {
  if (/^null$/i.test(token)) {
    return { value: null, raw: "null" };
  }
  if (/^true$/i.test(token)) {
    return { value: "true", raw: "true" };
  }
  if (/^false$/i.test(token)) {
    return { value: "false", raw: "false" };
  }
  if (
    isQuote(token[0]) &&
    isQuote(token[token.length - 1]) &&
    token.length >= 2
  ) {
    const unquoted = token.slice(1, -1);
    return { value: unquoted, raw: `"${unquoted}"` };
  }
  return { value: token, raw: token };
}
