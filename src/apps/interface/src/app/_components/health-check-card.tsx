import { StatusBadge } from "@/components/shared/status-badge";
import { Typography } from "@/components/shared/typography";
import type { HealthStatus } from "@/lib/api/health";
import { TickerQHealthDetail } from "./ticker-q-health-detail";

interface HealthCheckCardProps {
  resource: string;
  label: string;
  status: HealthStatus;
  detail: string;
  data?: Record<string, unknown>;
}

export function HealthCheckCard({
  resource,
  label,
  status,
  detail,
  data,
}: HealthCheckCardProps) {
  const showTickerQDetail = data !== undefined && resource === "tickerQ";

  return (
    <div className="border border-border rounded-xl p-4 bg-card shadow-sm space-y-2">
      <div className="flex items-center justify-between gap-2">
        <Typography variant="large" className="truncate min-w-0">{label}</Typography>
        <StatusBadge status={status} />
      </div>
      {showTickerQDetail ? (
        <TickerQHealthDetail data={data} />
      ) : (
        <Typography variant="muted" className="truncate">
          {detail}
        </Typography>
      )}
    </div>
  );
}
