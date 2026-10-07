"use client";

import { useBreadcrumbStore } from "@/stores/breadcrumb-store";
import { useLayoutEffect } from "react";

const FAILED_LABEL = "Not found";

export function useBreadcrumbLabel(segment: string, label: string | undefined, failed = false) {
  const setLabel = useBreadcrumbStore((state) => state.setLabel);
  const clearLabel = useBreadcrumbStore((state) => state.clearLabel);
  const resolved = label ?? (failed ? FAILED_LABEL : undefined);

  useLayoutEffect(() => {
    if (resolved === undefined) {
      return;
    }
    setLabel(segment, resolved);
    return () => clearLabel(segment);
  }, [segment, resolved, setLabel, clearLabel]);
}
