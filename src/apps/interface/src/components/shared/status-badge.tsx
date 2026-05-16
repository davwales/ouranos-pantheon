import { Badge } from "@/components/ui/badge";
import type { HealthStatus } from "@/lib/api/health";
import { cn } from "@/lib/utils";

interface StatusBadgeProps {
  status: HealthStatus | null;
}

const STATUS_CLASSES: Record<HealthStatus, string> = {
  Healthy: "bg-green-100 text-green-800",
  Degraded: "bg-amber-100 text-amber-800",
  Unhealthy: "bg-red-100 text-red-800",
  NotConfigured: "bg-gray-100 text-gray-500",
};

export function StatusBadge({ status }: StatusBadgeProps) {
  if (!status) {
    return null;
  }

  return <Badge className={cn(STATUS_CLASSES[status])}>{status}</Badge>;
}
