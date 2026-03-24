"use client";

import React, { useEffect, useRef } from "react";
import { createPortal } from "react-dom";

const FooterId = "footer-root";

export function Footer(props: React.ComponentProps<"div">) {
  const footerRef = useRef<HTMLDivElement>(null);
  return <div {...props} ref={footerRef} id={FooterId} />;
}

export function FooterContent({ children }: { children: React.ReactNode }) {
  const [mounted, setMounted] = React.useState(false);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setMounted(true);
    return () => {
      setMounted(false);
    };
  }, []);

  if (!mounted) return null;

  const footerRoot = document.getElementById(FooterId);

  if (!footerRoot) {
    console.warn("Footer root not found");
    return null;
  }

  return createPortal(children, footerRoot);
}
