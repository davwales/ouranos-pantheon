export function ScoreBar({ score }: { score: number }) {
  const width = `${Math.round(Math.min(Math.max(score, 0), 1) * 100)}%`;

  return (
    <div className="flex items-center gap-2">
      <div className="h-1.5 w-16 rounded-full bg-muted overflow-hidden">
        <div className="h-full rounded-full bg-primary" style={{ width }} />
      </div>
      <span className="text-xs tabular-nums text-muted-foreground">
        {score.toFixed(2)}
      </span>
    </div>
  );
}
