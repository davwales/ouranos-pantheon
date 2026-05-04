import { Typography } from "@/app/components/typography";
import { Progress } from "@/components/ui/progress";
import { BacktestStatus } from "@/lib/api/plutus";

interface InProgressBacktestViewProps {
  status: BacktestStatus;
  progressPercent: number;
  progressMessage: string;
}

export function InProgressBacktestView({
  status,
  progressPercent,
  progressMessage,
}: InProgressBacktestViewProps) {
  const isPending = status === "Pending";

  return (
    <div className="flex flex-col items-center justify-center py-16 text-muted-foreground">
      <div className="w-full max-w-md space-y-6">
        <div className="flex flex-col items-center space-y-3">
          {!isPending && (
            <Progress value={progressPercent} className="w-full" />
          )}
          {isPending && (
            <div className="flex items-center gap-2">
              <div className="h-2 w-2 animate-pulse rounded-full bg-muted-foreground" />
              <div
                className="h-2 w-2 animate-pulse rounded-full bg-muted-foreground"
                style={{ animationDelay: "0.2s" }}
              />
              <div
                className="h-2 w-2 animate-pulse rounded-full bg-muted-foreground"
                style={{ animationDelay: "0.4s" }}
              />
            </div>
          )}
          <Typography variant="h3">
            {isPending ? "Backtest is pending..." : "Backtest is running..."}
          </Typography>
          <p className="text-sm text-center">{progressMessage}</p>
          {!isPending && (
            <p className="text-xs text-muted-foreground">
              {progressPercent}% complete
            </p>
          )}
          {isPending && (
            <p className="text-xs text-muted-foreground">
              This page will update automatically
            </p>
          )}
        </div>
      </div>
    </div>
  );
}
