import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { type AvailableLora } from "@/lib/api/hermes";
import { cn } from "@/lib/utils";

export function availableLoraLabel(lora: AvailableLora): string {
  return lora.title ?? lora.identifier;
}

export function AvailableLoraSelect({
  value,
  loras,
  takenIdentifiers,
  onChange,
  className,
}: {
  value: string;
  loras: AvailableLora[];
  takenIdentifiers: ReadonlySet<string>;
  onChange: (lora: AvailableLora) => void;
  className?: string;
}) {
  const isMissing = value !== "" && !loras.some((l) => l.identifier === value);

  return (
    <Select
      value={value}
      onValueChange={(identifier) => {
        const lora = loras.find((l) => l.identifier === identifier);
        if (lora) {
          onChange(lora);
        }
      }}
    >
      <SelectTrigger className={cn("min-w-0", className)} aria-label="LoRA">
        <SelectValue placeholder="Select a LoRA" />
      </SelectTrigger>
      <SelectContent>
        {isMissing && (
          <SelectItem value={value} disabled>
            {value} (unavailable)
          </SelectItem>
        )}
        {loras.map((lora) => {
          const isTaken =
            lora.identifier !== value && takenIdentifiers.has(lora.identifier);
          return (
            <SelectItem
              key={lora.identifier}
              value={lora.identifier}
              disabled={isTaken}
            >
              {availableLoraLabel(lora)}
              {isTaken && " (already added)"}
              {!isTaken && !lora.isCompatible && " (incompatible)"}
            </SelectItem>
          );
        })}
      </SelectContent>
    </Select>
  );
}
