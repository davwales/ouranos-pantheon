import { type ReasoningEffort } from "@/lib/api/hermes";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

const MODEL_DEFAULT_EFFORT = "default";

const REASONING_EFFORTS: ReasoningEffort[] = ["None", "Low", "Medium", "High"];

export function ReasoningEffortSelect({
  value,
  onChange,
  disabled,
  className,
}: {
  value: ReasoningEffort | null;
  onChange: (value: ReasoningEffort | null) => void;
  disabled?: boolean;
  className?: string;
}) {
  return (
    <Select
      value={value ?? MODEL_DEFAULT_EFFORT}
      onValueChange={(selected) =>
        onChange(
          selected === MODEL_DEFAULT_EFFORT ? null : (selected as ReasoningEffort),
        )
      }
      disabled={disabled}
    >
      <SelectTrigger className={className} aria-label="Reasoning effort">
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value={MODEL_DEFAULT_EFFORT}>Model default</SelectItem>
        {REASONING_EFFORTS.map((effort) => (
          <SelectItem key={effort} value={effort}>
            {effort}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
