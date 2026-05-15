import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

interface FormSkeletonProps {
  fields?: number;
  hasTitle?: boolean;
  checkboxes?: number;
  textareas?: number;
  className?: string;
}

export function FormSkeleton({
  fields = 4,
  hasTitle = true,
  checkboxes = 0,
  textareas = 0,
  className,
}: FormSkeletonProps) {
  const regularFields = Math.max(0, fields - textareas);
  return (
    <div className={cn("flex flex-col gap-4", className)} aria-hidden="true">
      {hasTitle && <Skeleton className="h-8 w-1/3 mb-2" />}
      {Array.from({ length: regularFields }).map((_, index) => (
        <div key={index} className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Skeleton className="h-5 w-24" />
          <Skeleton className="h-9 w-full" />
        </div>
      ))}
      {Array.from({ length: textareas }).map((_, index) => (
        <div key={`ta-${index}`} className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Skeleton className="h-5 w-24" />
          <Skeleton className="h-20 w-full" />
        </div>
      ))}
      {Array.from({ length: checkboxes }).map((_, index) => (
        <div key={`cb-${index}`} className="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
          <Skeleton className="h-5 w-20" />
          <Skeleton className="h-5 w-5" />
        </div>
      ))}
      <div className="h-px bg-border my-4" />
      <div className="grid grid-cols-1 gap-4 md:flex md:justify-between">
        <Skeleton className="h-9 w-full md:w-40" />
        <Skeleton className="h-9 w-full md:w-40" />
      </div>
    </div>
  );
}
