import { cn } from "@/lib/utils";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { TextSkeleton } from "./text-skeleton";

interface CardSkeletonProps {
  hasHeader?: boolean;
  contentLines?: number;
  hasFooter?: boolean;
  className?: string;
}

export function CardSkeleton({
  hasHeader = true,
  contentLines = 3,
  hasFooter = false,
  className,
}: CardSkeletonProps) {
  return (
    <Card className={cn("w-full", className)} aria-hidden="true">
      {hasHeader && (
        <CardHeader>
          <Skeleton className="h-4 w-2/3" />
          <Skeleton className="h-4 w-1/2" />
        </CardHeader>
      )}
      <CardContent>
        <TextSkeleton lines={contentLines} lastLineWidth="w-3/4" />
      </CardContent>
      {hasFooter && (
        <div className="flex items-center px-6">
          <Skeleton className="h-8 w-24" />
        </div>
      )}
    </Card>
  );
}
