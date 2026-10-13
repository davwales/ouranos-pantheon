import {
  type InputKind,
  type StrategyConfiguration,
  type SwingConfiguration,
} from "@/lib/api/plutus";
import { FLIP_FIELDS } from "./flip-constants";
import { type LockState } from "./lock-badge";
import { INPUT_KIND_LABELS, THRESHOLD_FIELDS } from "./strategy-constants";

export type LockableField = {
  key: string;
  label: string;
  value: number | null;
};

type SwingFieldKey =
  | "maxPositions"
  | "maxPositionPercent"
  | "holdPeriodDays"
  | "buyThreshold"
  | "sellThreshold";

const SWING_FIELDS: ReadonlyArray<{ key: SwingFieldKey; label: string }> = [
  { key: "maxPositions", label: "Max Positions" },
  { key: "maxPositionPercent", label: "Max Position Percent" },
  { key: "holdPeriodDays", label: "Hold Period Days" },
  ...THRESHOLD_FIELDS,
];

export function weightLockKey(kind: InputKind): string {
  return `weight.${kind}`;
}

function swingFields(configuration: SwingConfiguration): LockableField[] {
  return [
    ...SWING_FIELDS.map(({ key, label }) => ({
      key,
      label,
      value: configuration[key] ?? null,
    })),
    ...configuration.inputWeights
      .filter((w) => w.weight !== 0)
      .map((w) => ({
        key: weightLockKey(w.kind),
        label: `${INPUT_KIND_LABELS[w.kind]} Weight`,
        value: w.weight,
      })),
  ];
}

export function tunableFields(
  configuration: StrategyConfiguration,
): LockableField[] {
  if (configuration.mode === "Swing") {
    return swingFields(configuration);
  }
  return FLIP_FIELDS.filter((f) => f.isOptimized).map((f) => ({
    key: f.key,
    label: f.label,
    value: configuration[f.key],
  }));
}

export function isLocked(
  configuration: StrategyConfiguration,
  key: string,
): boolean {
  return configuration.lockedParameters?.includes(key) ?? false;
}

export function lockState(
  configuration: StrategyConfiguration,
  key: string,
): LockState | undefined {
  return isLocked(configuration, key) ? "locked" : undefined;
}

export function withLock<T extends StrategyConfiguration>(
  configuration: T,
  key: string,
  locked: boolean,
): T {
  const others = (configuration.lockedParameters ?? []).filter((k) => k !== key);
  return { ...configuration, lockedParameters: locked ? [...others, key] : others };
}

export function lockedFields(
  configuration: StrategyConfiguration,
): LockableField[] {
  return tunableFields(configuration).filter(
    (f) => f.value != null && isLocked(configuration, f.key),
  );
}

export function isFullyLocked(configuration: StrategyConfiguration): boolean {
  const fields = tunableFields(configuration);
  return fields.length > 0 && fields.every((f) => isLocked(configuration, f.key));
}

/** Drops locks the optimizer would reject: unset fields and inputs no longer used. */
export function withValidLocks<T extends StrategyConfiguration>(
  configuration: T,
): T {
  return {
    ...configuration,
    lockedParameters: lockedFields(configuration).map((f) => f.key),
  };
}
