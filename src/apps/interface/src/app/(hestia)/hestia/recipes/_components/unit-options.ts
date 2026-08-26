export const UNIT_OPTIONS = [
  "cup",
  "tbsp",
  "tsp",
  "ml",
  "l",
  "g",
  "kg",
  "oz",
  "lb",
  "pinch",
  "dash",
  "clove",
  "slice",
  "piece",
  "can",
  "bunch",
  "whole",
] as const;

export type UnitOption = (typeof UNIT_OPTIONS)[number];

export const OTHER_UNIT_VALUE = "__other__";

export function isCustomUnit(value: string): boolean {
  const trimmed = value.trim();
  return trimmed !== "" && !(UNIT_OPTIONS as readonly string[]).includes(trimmed);
}
