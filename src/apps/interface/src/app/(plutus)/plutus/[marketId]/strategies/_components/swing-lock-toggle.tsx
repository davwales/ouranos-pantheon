import { type SwingConfiguration } from "@/lib/api/plutus";
import { LockToggle } from "./lock-toggle";
import { isLocked, withLock } from "./parameter-locks";

type SwingLockToggleProps = {
  configuration: SwingConfiguration;
  fieldKey:
    | "maxPositions"
    | "maxPositionPercent"
    | "holdPeriodDays"
    | "buyThreshold"
    | "sellThreshold";
  label: string;
  onChange: (next: SwingConfiguration) => void;
};

export function SwingLockToggle({
  configuration,
  fieldKey,
  label,
  onChange,
}: SwingLockToggleProps) {
  const hasValue = configuration[fieldKey] != null;
  const locked = hasValue && isLocked(configuration, fieldKey);

  return (
    <LockToggle
      label={label}
      locked={locked}
      disabledReason={hasValue ? undefined : "Set a value to lock it"}
      onToggle={() => onChange(withLock(configuration, fieldKey, !locked))}
    />
  );
}
