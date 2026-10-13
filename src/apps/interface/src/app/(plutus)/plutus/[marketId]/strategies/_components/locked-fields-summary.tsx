import { type StrategyConfiguration } from "@/lib/api/plutus";
import { cn } from "@/lib/utils";
import { Lock, LockOpen } from "lucide-react";
import { isFullyLocked, lockedFields } from "./parameter-locks";

export function LockedFieldsSummary({
  configuration,
}: {
  configuration: StrategyConfiguration;
}) {
  const locked = lockedFields(configuration);
  const fullyLocked = isFullyLocked(configuration);
  const Icon = locked.length > 0 ? Lock : LockOpen;

  return (
    <div
      className={cn(
        "flex items-start gap-2 rounded-lg border p-3 text-sm",
        fullyLocked
          ? "border-destructive/50 bg-destructive/10 text-destructive"
          : "bg-muted/30 text-muted-foreground",
      )}
    >
      <Icon className="size-4 mt-0.5 shrink-0" />
      <p>
        {fullyLocked ? (
          "Every tunable field is locked, so there is nothing to optimize. Unlock a field in the strategy editor."
        ) : locked.length > 0 ? (
          <>
            <span className="font-medium text-foreground">Locked: </span>
            {locked
              .map((f) => `${f.label} ${f.value?.toLocaleString("en-US")}`)
              .join(" · ")}
            . Edit the strategy to change.
          </>
        ) : (
          "Nothing locked. Every tunable field will be searched. Lock fields in the strategy editor to keep them fixed."
        )}
      </p>
    </div>
  );
}
