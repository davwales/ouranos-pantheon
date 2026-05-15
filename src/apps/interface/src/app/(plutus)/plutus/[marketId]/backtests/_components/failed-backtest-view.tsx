import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { RotateCcw } from "lucide-react";

export function FailedBacktestView({
  errorMessage,
  onRestart,
  isRestarting = false,
}: {
  errorMessage: string | null | undefined;
  onRestart?: () => void;
  isRestarting?: boolean;
}) {
  return (
    <Card className="border-red-200 dark:border-red-800">
      <CardHeader>
        <CardTitle className="text-red-600 dark:text-red-400">
          Backtest Failed
        </CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground mb-4">
          {errorMessage ?? "An unknown error occurred."}
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
