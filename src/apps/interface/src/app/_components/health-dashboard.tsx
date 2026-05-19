"use client";

import { useCallback, useEffect, useState } from "react";
import {
  healthApi,
  type HealthSummary,
  type GetHealthResponseDto,
} from "@/lib/api/health";
import { HealthSummaryCard } from "./health-summary-card";

type ApiState<T> =
  | { status: "loading"; data: T | undefined }
  | { status: "success"; data: T }
  | { status: "error"; data: T | undefined; error: Error };

function transformHealthResponse(dto: GetHealthResponseDto): HealthSummary {
  const checks = Object.entries(dto.checks).map(([resource, check]) => ({
    resource,
    status: check.status,
    detail: check.description,
    data: check.data,
  }));
  return {
    overallStatus: dto.status,
    checks,
    lastCheckedAt: new Date().toISOString(),
  };
}

const POLLING_INTERVAL_MS = 30_000;

export function HealthDashboard() {
  const [state, setState] = useState<ApiState<HealthSummary>>({
    status: "loading",
    data: undefined,
  });

  const fetchHealth = useCallback(async () => {
    setState((prev) => ({ ...prev, status: "loading" }));
    try {
      const dto = await healthApi.getHealthSummary();
      setState({ status: "success", data: transformHealthResponse(dto) });
    } catch (err) {
      setState((prev) => ({
        status: "error",
        data: prev.data,
        error:
          err instanceof Error ? err : new Error("Failed to fetch health"),
      }));
    }
  }, []);

  useEffect(() => {
    fetchHealth();
    const interval = setInterval(fetchHealth, POLLING_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [fetchHealth]);

  const isLoading = state.status === "loading";
  const data = state.data;
  const error = state.status === "error" ? state.error : null;

  return (
    <HealthSummaryCard
      overallStatus={data?.overallStatus ?? null}
      checks={data?.checks ?? null}
      isLoading={isLoading}
      error={error}
      onRetry={fetchHealth}
      lastCheckedAt={data?.lastCheckedAt ?? null}
    />
  );
}
