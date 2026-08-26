import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { RotateCcw } from "lucide-react";

export type RecipeImportFailedViewProps = {
  errorMessage: string | null | undefined;
  onReimport?: () => void;
  isReimporting?: boolean;
};

export function RecipeImportFailedView({
  errorMessage,
  onReimport,
  isReimporting = false,
}: RecipeImportFailedViewProps) {
  return (
    <Card className="border-red-200 dark:border-red-800">
      <CardHeader>
        <CardTitle className="text-red-600 dark:text-red-400">
          Import Failed
        </CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground mb-4">
          {errorMessage ?? "An unknown error occurred."}
        </p>
        {onReimport && (
          <Button onClick={onReimport} disabled={isReimporting} variant="outline">
            <RotateCcw className="w-4 h-4 mr-2" />
            {isReimporting ? "Retrying..." : "Retry Import"}
          </Button>
        )}
      </CardContent>
    </Card>
  );
}
