"use client";

import { Button } from "@/components/ui/button";

export function FolderErrorBanner({
  message,
  onRetry,
}: {
  message: string;
  onRetry?: () => void;
}) {
  return (
    <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive flex items-center justify-between">
      <span>{message}</span>
      {onRetry && (
        <Button variant="outline" size="sm" onClick={onRetry}>
          Retry
        </Button>
      )}
    </div>
  );
}
