import { type PaletteEntry, type PaletteGroup } from "./command-palette-types";

function searchableText(entry: PaletteEntry): string {
  return [entry.label, entry.hint, ...(entry.keywords ?? [])]
    .filter(Boolean)
    .join(" ")
    .toLowerCase();
}

export function filterGroups(
  groups: PaletteGroup[],
  query: string,
): PaletteGroup[] {
  const tokens = query.toLowerCase().split(/\s+/).filter(Boolean);
  if (tokens.length === 0) {
    return groups.filter((group) => group.entries.length > 0);
  }

  return groups
    .map((group) => ({
      ...group,
      entries: group.entries.filter((entry) => {
        const text = searchableText(entry);
        return tokens.every((token) => text.includes(token));
      }),
    }))
    .filter((group) => group.entries.length > 0);
}
