import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { isNotFound } from "@/lib/api-client";
import { AlertTriangle, FileQuestion } from "lucide-react";
import Link from "next/link";

type NotFoundCardProps = {
  title?: string;
  message?: string;
  backHref: string;
  backLabel?: string;
  /** The failed load; anything but a 404 renders a retryable error instead of "not found". */
  error?: Error;
  onRetry?: () => void;
};

export function NotFoundCard({
  title = "Not found",
  message = "The item you're looking for doesn't exist or has been removed.",
  backHref,
  backLabel = "Go back",
  error,
  onRetry,
}: NotFoundCardProps) {
  const failed = error !== undefined && !isNotFound(error);
  const Icon = failed ? AlertTriangle : FileQuestion;

  return (
    <div
      className="flex flex-col items-center justify-center min-h-full gap-4 p-4"
      role="alert"
      aria-live="polite"
    >
      <Card className="max-w-md w-full">
        <CardContent className="flex flex-col items-center gap-4 p-6 text-center">
          <Icon className="h-10 w-10 text-muted-foreground" />
          <h2 className="text-lg font-semibold">
            {failed ? "Couldn’t load this page" : title}
          </h2>
          <p className="text-sm text-muted-foreground">
            {failed ? error.message : message}
          </p>
          <div className="flex gap-2">
            {failed && onRetry && <Button onClick={onRetry}>Retry</Button>}
            <Button asChild variant="outline">
              <Link href={backHref}>{backLabel}</Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
