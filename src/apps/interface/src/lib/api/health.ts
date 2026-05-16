import { api } from "@/lib/api-client";

export type HealthStatus = "Healthy" | "Degraded" | "Unhealthy" | "NotConfigured";

export interface HealthCheckResultDto {
  status: HealthStatus;
  description: string;
  timestamp: string;
}

export interface GetHealthResponseDto {
  status: HealthStatus;
  checks: Record<string, HealthCheckResultDto>;
}

export interface HealthCheckRow {
  resource: string;
  status: HealthStatus;
  detail: string;
}

export interface HealthSummary {
  overallStatus: HealthStatus;
  checks: HealthCheckRow[];
  lastCheckedAt: string;
}

export const healthApi = {
  getHealthSummary: () => api.get<GetHealthResponseDto>("/health"),
};
