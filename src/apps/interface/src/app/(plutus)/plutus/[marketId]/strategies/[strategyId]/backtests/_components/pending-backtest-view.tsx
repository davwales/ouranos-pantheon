import { Typography } from "@/components/shared/typography";
import { RefreshCw } from "lucide-react";

export function PendingBacktestView() {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-muted-foreground">
      <RefreshCw className="animate-spin w-8 h-8 mb-4" />
      <Typography variant="h3">Backtest is pending...</Typography>
      <p className="text-sm mt-2">Waiting to be queued for execution.</p>
    </div>
  );
}
