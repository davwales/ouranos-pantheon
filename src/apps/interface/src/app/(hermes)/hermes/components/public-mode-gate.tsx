"use client";

import { usePublicMode } from "@/app/(hermes)/hermes/components/public-mode-context";
import { NotFoundCard } from "@/components/shared/not-found-card";
import React from "react";

/** Renders its children only outside public mode; create pages use it. */
export function PublicModeGate({
  backHref,
  backLabel,
  children,
}: React.PropsWithChildren<{ backHref: string; backLabel: string }>) {
  const isPublicMode = usePublicMode();

  if (isPublicMode) {
    return (
      <NotFoundCard
        title="Not available"
        message="Hermes is in public mode, so nothing can be created or changed."
        backHref={backHref}
        backLabel={backLabel}
      />
    );
  }

  return children;
}
