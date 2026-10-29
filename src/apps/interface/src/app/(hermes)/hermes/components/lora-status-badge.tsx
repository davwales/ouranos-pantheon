import { Badge } from "@/components/ui/badge";
import { type Lora } from "@/lib/api/hermes";
import { cn } from "@/lib/utils";
import { AlertTriangle } from "lucide-react";

export function LoraStatusBadge({
  lora,
  className,
}: {
  lora: Pick<Lora, "isUnavailable" | "isIncompatible">;
  className?: string;
}) {
  if (lora.isUnavailable) {
    return (
      <Badge
        variant="destructive"
        className={cn("gap-1", className)}
        title="No longer on the image server, so it isn't applied."
      >
        <AlertTriangle className="h-3 w-3" />
        Unavailable
      </Badge>
    );
  }

  if (lora.isIncompatible) {
    return (
      <Badge
        variant="outline"
        className={cn(
          "gap-1 border-transparent bg-amber-500/15 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300",
          className,
        )}
        title="Made for a different kind of image model; results may be poor."
      >
        <AlertTriangle className="h-3 w-3" />
        Incompatible
      </Badge>
    );
  }

  return null;
}
