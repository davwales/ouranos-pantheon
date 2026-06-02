import { EyeOff } from "lucide-react";
import * as React from "react";

export function getVisibilityInfo(isPublic: boolean) {
  const icons: React.ReactNode[] = [];
  const labels: string[] = [];

  if (!isPublic) {
    icons.push(<EyeOff key="eye" className="size-3.5 text-muted-foreground" />);
    labels.push("Private");
  }

  return { icons, label: labels.join(" & ") || null };
}
