"use client";

import { useBreadcrumbStore } from "@/stores/breadcrumb-store";
import { useLayoutEffect } from "react";

const FAILED_LABEL = "Not found";

export function useBreadcrumbLabel(segment: string, label: string | undefined, failed = false) {
  const setLabel = useBreadcrumbStore((state) => state.setLabel);
  const resolved = label ?? (failed ? FAILED_LABEL : undefined);

  // Labels outlive the page that registered them so a detail page's origin crumbs (e.g. a group) stay named.
  useLayoutEffect(() => {
    if (resolved === undefined) {
      return;
    }
    setLabel(segment, resolved);
  }, [segment, resolved, setLabel]);
}
