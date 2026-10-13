import { LockBadge, type LockState } from "./lock-badge";

const LOCK_DESCRIPTIONS: Record<LockState, string> = {
  locked: "Locked: the optimizer keeps this value",
  unlocked: "Unlocked: the optimizer may change this value",
  fixed: "Kept as set: never optimized",
};

export function ConfigRow({
  label,
  value,
  lock,
}: {
  label: string;
  value: string | number | null | undefined;
  lock?: LockState;
}) {
  if (value == null) return null;
  return (
    <div className="flex justify-between items-center py-2 border-b border-border/50">
      <span className="text-sm text-muted-foreground">{label}</span>
      <span className="text-sm font-medium inline-flex items-center gap-2">
        {lock && (
          <span
            role="img"
            aria-label={LOCK_DESCRIPTIONS[lock]}
            title={LOCK_DESCRIPTIONS[lock]}
          >
            <LockBadge state={lock} />
          </span>
        )}
        {String(value)}
      </span>
    </div>
  );
}
