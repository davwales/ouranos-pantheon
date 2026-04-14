import type { GapInfo } from "./chart-utils";

interface GapAxisTickProps {
  x?: number | string;
  y?: number | string;
  payload?: { value: number };
  gaps: GapInfo[];
  tickFormatter: (index: number) => string;
  [key: string]: unknown;
}

export function GapAxisTick({
  x: rawX = 0,
  y: rawY = 0,
  payload,
  gaps,
  tickFormatter,
}: GapAxisTickProps) {
  const x = Number(rawX);
  const y = Number(rawY);

  if (!payload) return null;

  const index = payload.value;
  const hasGapBefore = gaps.some((g) => g.afterIndex === index - 1);
  const label = `${hasGapBefore ? "// " : ""}${tickFormatter(index)}`;

  return (
    <g transform={`translate(${x},${y})`}>
      <text
        x={0}
        y={0}
        dy={16}
        textAnchor="middle"
        fill="currentColor"
        fontSize={10}
      >
        {label}
      </text>
    </g>
  );
}
