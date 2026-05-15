import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { RotateCcw } from "lucide-react";

export function CancelledBacktestView({
  errorMessage,
  onRestart,
  isRestarting = false,
}: {
  errorMessage: string | null | undefined;
  onRestart?: () => void;
  isRestarting?: boolean;
}) {
  return (
    <Card className="border-orange-200 dark:border-orange-800">
      <CardHeader>
        <CardTitle className="text-orange-600 dark:text-orange-400">
          Backtest Cancelled
        </CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground mb-4">
          {errorMessage ?? "This backtest was cancelled."}
        </p>
        {onRestart && (
          <Button onClick={onRestart} disabled={isRestarting} variant="outline">
            <RotateCcw className="w-4 h-4 mr-2" />
            {isRestarting ? "Restarting..." : "Restart Backtest"}
          </Button>
        )}
      </CardContent>
    </Card>
  );
}
