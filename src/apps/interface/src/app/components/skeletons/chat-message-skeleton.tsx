import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

interface ChatMessageSkeletonProps {
  pairCount?: number;
  className?: string;
}

export function ChatMessageSkeleton({
  pairCount = 3,
  className,
}: ChatMessageSkeletonProps) {
  return (
    <div className={cn("space-y-4", className)} aria-hidden="true">
      {Array.from({ length: pairCount }).map((_, index) => (
        <div key={index} className="space-y-4">
          <div className="flex justify-end">
            <div className="max-w-[70%]">
              <div className="py-2 px-4 border rounded-2xl bg-accent/30">
                <Skeleton className="h-4 w-full" />
              </div>
              <Skeleton className="h-3 w-16 ml-auto mr-2.5 mt-1" />
            </div>
          </div>
          <div className="flex justify-start">
            <div className="max-w-[70%]">
              <div className="py-2 px-4 border rounded-2xl">
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-3/4 mt-2" />
              </div>
              <Skeleton className="h-3 w-20 ml-2.5 mt-1" />
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
