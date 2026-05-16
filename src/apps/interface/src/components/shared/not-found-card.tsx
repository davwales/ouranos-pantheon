"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { FileQuestion } from "lucide-react";
import Link from "next/link";

interface NotFoundCardProps {
  title?: string;
  message?: string;
  backHref: string;
  backLabel?: string;
}

export function NotFoundCard({
  title = "Not found",
  message = "The item you're looking for doesn't exist or has been removed.",
  backHref,
  backLabel = "Go back",
}: NotFoundCardProps) {
  return (
    <div
      className="flex flex-col items-center justify-center min-h-full gap-4 p-4"
      role="alert"
      aria-live="polite"
    >
      <Card className="max-w-md w-full">
        <CardContent className="flex flex-col items-center gap-4 p-6 text-center">
          <FileQuestion className="h-10 w-10 text-muted-foreground" />
          <h2 className="text-lg font-semibold">{title}</h2>
          <p className="text-sm text-muted-foreground">{message}</p>
          <Button asChild variant="outline">
            <Link href={backHref}>{backLabel}</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}