import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";
import { useMemo } from "react";

interface InfoCardSkeletonProps {
  hasIcon?: boolean;
  descriptionLines?: number;
  className?: string;
}

export function InfoCardSkeleton({
  hasIcon = false,
  descriptionLines = 1,
  className,
}: InfoCardSkeletonProps) {
  return (
    <div
      className={cn(
        "flex items-start rounded-4xl border-2 border-accent py-4 px-3",
        className,
      )}
      aria-hidden="true"
    >
      {hasIcon && <div className="shrink-0 w-20 h-20 rounded-2xl bg-muted-foreground/15" />}
      <div className="ml-4 flex flex-col gap-1 flex-1 min-w-0">
        <Skeleton className="h-3.5 w-2/3" />
        {descriptionLines > 0 &&
          Array.from({ length: descriptionLines }).map((_, index) => (
            <Skeleton
              key={index}
              className={cn(
                "h-3.5",
                index === descriptionLines - 1 && descriptionLines > 1
                  ? "w-3/4"
                  : "w-full",
              )}
            />
          ))}
      </div>
    </div>
  );
}

const MD_COL_MAP: Record<number, string> = {
  1: "md:grid-cols-1",
  2: "md:grid-cols-2",
  3: "md:grid-cols-3",
  4: "md:grid-cols-4",
};

const LG_COL_MAP: Record<number, string> = {
  1: "lg:grid-cols-1",
  2: "lg:grid-cols-2",
  3: "lg:grid-cols-3",
  4: "lg:grid-cols-4",
  5: "lg:grid-cols-5",
  6: "lg:grid-cols-6",
};

interface InfoCardGridSkeletonProps {
  count?: number;
  hasIcon?: boolean;
  cols?: { md?: number; lg?: number };
  className?: string;
}

export function InfoCardGridSkeleton({
  count = 6,
  hasIcon = false,
  cols,
  className,
}: InfoCardGridSkeletonProps) {
  const mdColClass = MD_COL_MAP[cols?.md ?? 2] ?? "md:grid-cols-2";
  const lgColClass = LG_COL_MAP[cols?.lg ?? 3] ?? "lg:grid-cols-3";

  return (
    <div
      className={cn(
        "grid grid-cols-1",
        mdColClass,
        lgColClass,
        "gap-4",
        className,
      )}
      aria-hidden="true"
    >
      {Array.from({ length: count }).map((_, i) => (
        <InfoCardSkeleton key={i} hasIcon={hasIcon} />
      ))}
    </div>
  );
}