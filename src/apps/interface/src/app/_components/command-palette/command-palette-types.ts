import { type ThemeOption } from "@/components/shared/theme-toggle";
import { type LucideIcon } from "lucide-react";

export type PaletteTarget =
  | { kind: "link"; href: string }
  | { kind: "theme"; theme: ThemeOption["value"] };

export type PaletteEntry = {
  id: string;
  label: string;
  hint?: string;
  keywords?: string[];
  target: PaletteTarget;
};

export type PaletteGroup = {
  heading: string;
  icon: LucideIcon;
  entries: PaletteEntry[];
};
