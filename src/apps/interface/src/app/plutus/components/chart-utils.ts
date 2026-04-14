export interface GapInfo {
  afterIndex: number;
  durationMs: number;
}

export interface ProcessedChartData<T extends { date: Date }> {
  data: Array<T & { _chartIndex: number }>;
  gaps: GapInfo[];
  medianIntervalMs: number;
}

export function processChartData<T extends { date: Date }>(
  rawData: T[],
  gapThresholdMultiplier = 3,
): ProcessedChartData<T> {
  if (rawData.length < 2) {
    return {
      data: rawData.map((d, i) => ({ ...d, _chartIndex: i })),
      gaps: [],
      medianIntervalMs: 0,
    };
  }

  const sorted = [...rawData].sort(
    (a, b) => a.date.getTime() - b.date.getTime(),
  );

  const intervals: number[] = [];
  for (let i = 1; i < sorted.length; i++) {
    intervals.push(sorted[i].date.getTime() - sorted[i - 1].date.getTime());
  }

  const sortedIntervals = [...intervals].sort((a, b) => a - b);
  const mid = Math.floor(sortedIntervals.length / 2);
  const medianIntervalMs =
    sortedIntervals.length % 2 === 0
      ? (sortedIntervals[mid - 1] + sortedIntervals[mid]) / 2
      : sortedIntervals[mid];

  const threshold = medianIntervalMs * gapThresholdMultiplier;

  const gaps: GapInfo[] = [];
  for (let i = 0; i < intervals.length; i++) {
    if (intervals[i] > threshold) {
      gaps.push({ afterIndex: i, durationMs: intervals[i] });
    }
  }

  const data = sorted.map((d, i) => ({ ...d, _chartIndex: i }));

  return { data, gaps, medianIntervalMs };
}
