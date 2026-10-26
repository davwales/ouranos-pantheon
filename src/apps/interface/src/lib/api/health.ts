import { ApiError, apiUrl } from "@/lib/api-client";

export type HealthStatus = "Healthy" | "Degraded" | "Unhealthy" | "NotConfigured";

export interface HealthCheckResultDto {
  status: HealthStatus;
  description: string;
  timestamp: string;
  data?: Record<string, unknown>;
}

export interface GetHealthResponseDto {
  status: HealthStatus;
  checks: Record<string, HealthCheckResultDto>;
}

export interface HealthCheckRow {
  resource: string;
  status: HealthStatus;
  detail: string;
  data?: Record<string, unknown>;
}

export interface HealthSummary {
  overallStatus: HealthStatus;
  checks: HealthCheckRow[];
  lastCheckedAt: string;
}

export const healthApi = {
  getHealthSummary: async (): Promise<GetHealthResponseDto> => {
    const res = await fetch(apiUrl("/health"));
    // 503 means a dependency is unhealthy; the body still carries the per-check report.
    if (!res.ok && res.status !== 503) {
      throw new ApiError(res.status, res.statusText);
    }
    return res.json() as Promise<GetHealthResponseDto>;
  },
};
