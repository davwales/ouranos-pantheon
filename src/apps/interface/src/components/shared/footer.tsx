"use client";

import React, { useRef, useSyncExternalStore } from "react";
import { createPortal } from "react-dom";

const FooterId = "footer-root";

export function Footer(props: React.ComponentProps<"div">) {
  const footerRef = useRef<HTMLDivElement>(null);
  return (
    <div
      {...props}
      ref={footerRef}
      id={FooterId}
      style={{ paddingBottom: "env(safe-area-inset-bottom)" }}
    />
  );
}

export function FooterContent({ children }: { children: React.ReactNode }) {
  const mounted = useSyncExternalStore(
    () => () => {},
    () => true,
    () => false,
  );

  if (!mounted) return null;

  const footerRoot = document.getElementById(FooterId);

  if (!footerRoot) {
    console.warn("Footer root not found");
    return null;
  }

  return createPortal(children, footerRoot);
}
