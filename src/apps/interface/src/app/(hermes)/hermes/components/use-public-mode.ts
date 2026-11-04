"use client";

import { useFeatureFlag } from "@/components/shared/feature-flags-context";

export const PUBLIC_MODE_FLAG = "hermes.public_mode";

/**
 * Whether Hermes is read-only, so pages should hide create, edit, save and delete controls.
 * Unknown counts as on, like the gateway, so write controls never flash up for visitors.
 */
export function usePublicMode(): boolean {
  return useFeatureFlag(PUBLIC_MODE_FLAG, { whileUnknown: true });
}
