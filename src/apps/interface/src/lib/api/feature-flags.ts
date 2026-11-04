import { api } from "@/lib/api-client";

export type FeatureFlags = {
  flags: Record<string, boolean>;
};

export const featureFlagsApi = {
  getAll: () => api.get<FeatureFlags>("/api/feature-flags"),
};
