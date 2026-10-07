"use client";

import { useBreadcrumbLabel } from "./use-breadcrumb-label";

export function BreadcrumbLabel({ segment, label }: { segment: string; label: string }) {
  useBreadcrumbLabel(segment, label);
  return null;
}
