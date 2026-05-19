import { StatusBadge } from "@/components/shared/status-badge";
import { HealthDashboardSkeleton } from "@/components/shared/skeletons/health-dashboard-skeleton";
import { Typography } from "@/components/shared/typography";
import { Button } from "@/components/ui/button";
import type { HealthStatus, HealthCheckRow } from "@/lib/api/health";
import { HealthCheckCard } from "./health-check-card";

interface HealthSummaryCardProps {
  overallStatus: HealthStatus | null;
  checks: HealthCheckRow[] | null;
  isLoading: boolean;
  error: Error | null;
  onRetry: () => void;
  lastCheckedAt?: string | null;
}

const RESOURCE_LABELS: Record<string, string> = {
  postgres: "PostgreSQL",
  rabbitmq: "RabbitMQ",
  ouranosMl: "Ouranos ML",
  websockets: "WebSockets",
  tickerQ: "TickerQ Jobs",
};

export function HealthSummaryCard({
  overallStatus,
  checks,
  isLoading,
  error,
  onRetry,
  lastCheckedAt,
}: HealthSummaryCardProps) {
  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2 justify-between flex-wrap">
        <div className="flex items-center gap-2 flex-wrap">
          <Typography variant="h4">System Health</Typography>
          {overallStatus && <StatusBadge status={overallStatus} />}
        </div>
        {lastCheckedAt && (
          <Typography variant="muted">
            Last checked: {new Date(lastCheckedAt).toLocaleString()}
          </Typography>
        )}
      </div>

      {error && checks && (
        <Typography variant="muted" className="text-amber-600">
          Last update failed - showing cached data.
        </Typography>
      )}

      {isLoading && !checks && <HealthDashboardSkeleton />}

      {error && !checks && !isLoading && (
        <div className="space-y-2">
          <Typography variant="muted" className="text-destructive">
            Failed to fetch health status.
          </Typography>
          <Button variant="outline" size="sm" onClick={onRetry}>
            Retry
          </Button>
        </div>
      )}

      {checks && checks.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {checks.map((check) => (
            <HealthCheckCard
              key={check.resource}
              resource={check.resource}
              label={RESOURCE_LABELS[check.resource] ?? check.resource}
              status={check.status}
              detail={check.detail}
              data={check.data}
            />
          ))}
        </div>
      )}

      {checks && checks.length === 0 && (
        <Typography variant="muted">No health checks registered.</Typography>
      )}
    </div>
  );
}
