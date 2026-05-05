export function ContextUsageBar({
  tokenUsage,
  contextWindow,
}: {
  tokenUsage: {
    inputTokens: number;
    outputTokens: number;
    totalTokens: number;
  };
  contextWindow: number;
}) {
  const pct = (tokenUsage.totalTokens / contextWindow) * 100;
  const barColor =
    pct >= 100 ? "bg-red-500" : pct >= 75 ? "bg-yellow-500" : "bg-primary";
  const textColor =
    pct >= 100
      ? "text-red-500"
      : pct >= 75
        ? "text-yellow-500"
        : "text-muted-foreground";

  return (
    <div className="flex items-center gap-2 px-2 py-1">
      <div className="flex-1 h-1.5 rounded-full bg-secondary overflow-hidden">
        <div
          className={`h-full rounded-full transition-all ${barColor}`}
          style={{ width: `${Math.min(pct, 100)}%` }}
        />
      </div>
      <p className={`text-xs font-mono shrink-0 ${textColor}`}>
        {tokenUsage.totalTokens.toLocaleString()} /{" "}
        {contextWindow.toLocaleString()} ({pct.toFixed(1)}%)
      </p>
    </div>
  );
}
