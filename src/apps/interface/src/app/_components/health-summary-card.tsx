import { StatusBadge } from "@/components/shared/status-badge";
import { HealthDashboardSkeleton } from "@/components/shared/skeletons/health-dashboard-skeleton";
import { Typography } from "@/components/shared/typography";
import { Button } from "@/components/ui/button";
import type { HealthStatus, HealthCheckRow } from "@/lib/api/health";

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
      {/* Header row with title, overall badge, and last-checked timestamp */}
      <div className="flex items-center gap-2 justify-between flex-wrap">
        <div className="flex items-center gap-2 flex-wrap">
          <Typography variant="lead">System Health</Typography>
          {overallStatus && <StatusBadge status={overallStatus} />}
        </div>
        {lastCheckedAt && (
          <Typography variant="muted">
            Last checked: {new Date(lastCheckedAt).toLocaleString()}
          </Typography>
        )}
      </div>

      {/* Stale-data warning */}
      {error && checks && (
        <Typography variant="muted" className="text-amber-600">
          Last update failed — showing cached data.
        </Typography>
      )}

      {/* Loading state */}
      {isLoading && !checks && <HealthDashboardSkeleton />}

      {/* Error state with no cached data */}
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

      {/* Health check rows */}
      {checks && checks.length > 0 && (
        <div className="space-y-0 divide-y">
          {checks.map((check) => (
            <div
              key={check.resource}
              className="grid grid-cols-1 md:grid-cols-[1fr_auto_1fr] gap-2 md:gap-4 py-3 items-center"
            >
              <Typography variant="small">
                {RESOURCE_LABELS[check.resource] ?? check.resource}
              </Typography>
              <StatusBadge status={check.status} />
              <Typography variant="muted" className="md:text-right">
                {check.detail}
              </Typography>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
