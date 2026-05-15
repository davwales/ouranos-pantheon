import { Typography } from "@/components/shared/typography";
import { Button } from "@/components/ui/button";
import { RefreshCw } from "lucide-react";

export function RunningBacktestView({ onRefresh }: { onRefresh: () => void }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-muted-foreground">
      <RefreshCw className="animate-spin w-8 h-8 mb-4" />
      <Typography variant="h3">Backtest is running...</Typography>
      <p className="text-sm mt-2">
        Results will appear once the simulation completes.
      </p>
      <Button variant="outline" className="mt-4" onClick={onRefresh}>
        <RefreshCw className="w-4 h-4 mr-1" />
        Refresh
      </Button>
    </div>
  );
}
