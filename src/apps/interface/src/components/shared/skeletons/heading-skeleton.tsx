import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

const VARIANT_HEIGHTS: Record<string, string> = {
  h1: "h-8",
  h2: "h-7",
  h3: "h-6",
  h4: "h-5",
};

interface HeadingSkeletonProps {
  variant?: keyof typeof VARIANT_HEIGHTS;
  width?: string;
  className?: string;
}

export function HeadingSkeleton({
  variant = "h2",
  width = "w-1/3",
  className,
}: HeadingSkeletonProps) {
  return (
    <Skeleton
      className={cn(VARIANT_HEIGHTS[variant], width, className)}
      aria-hidden="true"
    />
  );
}