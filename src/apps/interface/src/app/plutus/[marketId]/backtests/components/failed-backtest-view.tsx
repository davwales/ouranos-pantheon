import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export function FailedBacktestView({
  errorMessage,
}: {
  errorMessage: string | null | undefined;
}) {
  return (
    <Card className="border-red-200 dark:border-red-800">
      <CardHeader>
        <CardTitle className="text-red-600 dark:text-red-400">
          Backtest Failed
        </CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground">
          {errorMessage ?? "An unknown error occurred."}
        </p>
      </CardContent>
    </Card>
  );
}
