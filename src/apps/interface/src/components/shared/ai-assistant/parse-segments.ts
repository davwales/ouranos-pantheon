export type Segment =
  | { type: "loading" }
  | { type: "thinking"; content: string; isStreaming: boolean }
  | { type: "text"; content: string };

interface TagDefinition {
  openTag: string;
  closeTag: string;
  createSegment: (content: string, isStreaming: boolean) => Segment;
}

const TAG_DEFINITIONS: TagDefinition[] = [
  {
    openTag: "<think>",
    closeTag: "</think>",
    createSegment: (content, isStreaming) => ({
      type: "thinking",
      content,
      isStreaming,
    }),
  },
];

function findEarliestTag(
  content: string,
  tagDefs: TagDefinition[],
): { def: TagDefinition; idx: number } | null {
  let earliest: { def: TagDefinition; idx: number } | null = null;
  for (const def of tagDefs) {
    const idx = content.indexOf(def.openTag);
    if (idx !== -1 && (earliest === null || idx < earliest.idx)) {
      earliest = { def, idx };
    }
  }
  return earliest;
}

function parseTaggedSegment(
  afterOpenTag: string,
  def: TagDefinition,
  isStreaming: boolean,
): { segment: Segment; remainder: string } {
  const closeIdx = afterOpenTag.indexOf(def.closeTag);
  if (closeIdx === -1) {
    return {
      segment: def.createSegment(afterOpenTag, isStreaming),
      remainder: "",
    };
  }
  return {
    segment: def.createSegment(afterOpenTag.slice(0, closeIdx), false),
    remainder: afterOpenTag.slice(closeIdx + def.closeTag.length),
  };
}

function stripPartialOpenTag(
  content: string,
  tagDefs: TagDefinition[],
): string {
  const allPrefixes = tagDefs
    .flatMap(({ openTag }) =>
      Array.from({ length: openTag.length - 1 }, (_, i) =>
        openTag.slice(0, i + 1),
      ),
    )
    .sort((a, b) => b.length - a.length);

  const match = allPrefixes.find((prefix) => content.endsWith(prefix));
  return match ? content.slice(0, content.length - match.length) : content;
}

export function parseSegments(
  content: string,
  isStreaming: boolean,
): Segment[] {
  if (content.length === 0) {
    return isStreaming ? [{ type: "loading" }] : [];
  }

  const segments: Segment[] = [];
  let remaining = content;

  while (remaining.length > 0) {
    const match = findEarliestTag(remaining, TAG_DEFINITIONS);

    if (!match) {
      const text = stripPartialOpenTag(remaining, TAG_DEFINITIONS);
      if (text.length > 0) {
        segments.push({ type: "text", content: text });
      }
      break;
    }

    if (match.idx > 0) {
      segments.push({ type: "text", content: remaining.slice(0, match.idx) });
    }

    const afterOpenTag = remaining.slice(match.idx + match.def.openTag.length);
    const { segment, remainder } = parseTaggedSegment(
      afterOpenTag,
      match.def,
      isStreaming,
    );
    segments.push(segment);
    remaining = remainder;
  }

  return segments;
}
