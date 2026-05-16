import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

interface TextSkeletonProps {
  lines?: number;
  gap?: number;
  lastLineWidth?: string;
  className?: string;
}

const GAP_CLASSES: Record<number, string> = {
  1: "space-y-1",
  2: "space-y-2",
  3: "space-y-3",
  4: "space-y-4",
  5: "space-y-5",
  6: "space-y-6",
};

export function TextSkeleton({
  lines = 1,
  gap = 2,
  lastLineWidth = "w-full",
  className,
}: TextSkeletonProps) {
  return (
    <div className={cn(GAP_CLASSES[gap] ?? `space-y-[${gap * 0.25}rem]`, className)} aria-hidden="true">
      {Array.from({ length: lines }).map((_, index) => (
        <Skeleton
          key={index}
          className={cn(
            "h-4",
            index === lines - 1 && lines > 1 ? lastLineWidth : "w-full",
          )}
        />
      ))}
    </div>
  );
}
