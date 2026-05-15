"use client";

import { Button } from "@/components/ui/button";
import { useEffect } from "react";
import { AlertCircle } from "lucide-react";
import Link from "next/link";

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <html lang="en">
      <body>
        <div
          className="flex flex-col items-center justify-center min-h-dvh gap-4 p-8"
          role="alert"
          aria-live="polite"
        >
          <AlertCircle className="h-10 w-10 text-destructive" />
          <h2 className="text-lg font-semibold text-destructive">
            Something went wrong
          </h2>
          <p className="text-sm text-muted-foreground max-w-md text-center">
            An unexpected error occurred. Please try again or return to the home
            page.
          </p>
          <div className="flex gap-3">
            <Button onClick={reset} variant="outline">
              Try again
            </Button>
            <Button asChild variant="outline">
              <Link href="/">Go home</Link>
            </Button>
          </div>
        </div>
      </body>
    </html>
  );
}