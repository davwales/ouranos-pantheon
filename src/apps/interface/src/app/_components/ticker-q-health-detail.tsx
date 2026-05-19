import { Typography } from "@/components/shared/typography";

interface TickerQData {
  healthy: number;
  failed: number;
  overdue: number;
  neverRan: number;
  total: number;
}

const DATA_KEYS: (keyof TickerQData)[] = ["healthy", "total", "overdue", "failed", "neverRan"];

const KEY_LABELS: Record<keyof TickerQData, string> = {
  healthy: "healthy",
  total: "total",
  overdue: "overdue",
  failed: "failed",
  neverRan: "never ran",
};

const KEY_COLORS: Record<keyof TickerQData, string> = {
  healthy: "text-green-600 dark:text-green-400",
  total: "text-muted-foreground",
  overdue: "text-amber-600 dark:text-amber-400",
  failed: "text-red-600 dark:text-red-400",
  neverRan: "text-muted-foreground",
};

export function TickerQHealthDetail({ data }: { data: Record<string, unknown> }) {
  const tickerData = data as unknown as TickerQData;
  const presentKeys = DATA_KEYS.filter((key) => Object.hasOwn(tickerData, key));

  if (presentKeys.length === 0) {
    return null;
  }

  return (
    <div
      className="flex items-center gap-2 flex-wrap"
      aria-label={`${tickerData.healthy || 0} healthy, ${tickerData.overdue || 0} overdue, ${tickerData.failed || 0} failed, ${tickerData.neverRan || 0} never ran`}
    >
      {presentKeys.map((key, index) => (
        <span key={key} className="flex items-center gap-2">
          {index > 0 && <span className="text-muted-foreground" aria-hidden="true">&middot;</span>}
          <Typography variant="small" className={KEY_COLORS[key]}>
            {tickerData[key]} {KEY_LABELS[key]}
          </Typography>
        </span>
      ))}
    </div>
  );
}
