import { Badge } from "@/components/ui/badge";
import type { HealthStatus } from "@/lib/api/health";
import { cn } from "@/lib/utils";

interface StatusBadgeProps {
  status: HealthStatus | null;
}

const STATUS_CLASSES: Record<HealthStatus, string> = {
  Healthy: "bg-green-500/15 text-green-700 dark:bg-green-500/20 dark:text-green-300",
  Degraded: "bg-amber-500/15 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300",
  Unhealthy: "bg-red-500/15 text-red-700 dark:bg-red-500/20 dark:text-red-300",
  NotConfigured: "bg-muted text-muted-foreground",
};

export function StatusBadge({ status }: StatusBadgeProps) {
  if (!status) {
    return null;
  }

  return <Badge className={cn(STATUS_CLASSES[status])}>{status}</Badge>;
}
