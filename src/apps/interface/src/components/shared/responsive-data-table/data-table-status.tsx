import { Button } from "@/components/ui/button";

type DataTableStatusProps = {
  error?: Error;
  onRetry?: () => void;
};

function RetryButton({ onRetry }: { onRetry?: () => void }) {
  if (!onRetry) return null;
  return (
    <Button variant="outline" size="sm" onClick={onRetry}>
      Retry
    </Button>
  );
}

/** Body of a table with no rows: "No results" only when the fetch succeeded. */
export function DataTableEmptyState({ error, onRetry }: DataTableStatusProps) {
  if (!error) return <>No results.</>;

  return (
    <div role="alert" className="flex flex-col items-center gap-2">
      <span className="text-destructive">
        Couldn’t load results: {error.message}
      </span>
      <RetryButton onRetry={onRetry} />
    </div>
  );
}

/** Warning above rows from an earlier fetch when the latest one failed. */
export function DataTableRefreshError({
  error,
  onRetry,
  hasRows,
}: DataTableStatusProps & { hasRows: boolean }) {
  if (!error || !hasRows) return null;

  return (
    <div
      role="alert"
      className="mt-2 flex items-center justify-between gap-2 rounded-md border border-destructive/50 bg-destructive/10 px-3 py-2 text-sm text-destructive"
    >
      <span>Couldn’t refresh; showing the last loaded results.</span>
      <RetryButton onRetry={onRetry} />
    </div>
  );
}
