import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { type ToolSetSummary } from "@/lib/api/hermes";

const NO_TOOL_SET = "none";

export function ToolSetSelect({
  value,
  toolSets,
  onChange,
  disabled,
  className,
  "aria-label": ariaLabel = "Tool set",
}: {
  value: string | null;
  toolSets: ToolSetSummary[];
  onChange: (value: string | null) => void;
  disabled?: boolean;
  className?: string;
  "aria-label"?: string;
}) {
  return (
    <Select
      value={value ?? NO_TOOL_SET}
      onValueChange={(selected) =>
        onChange(selected === NO_TOOL_SET ? null : selected)
      }
      disabled={disabled}
    >
      <SelectTrigger className={className} aria-label={ariaLabel}>
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value={NO_TOOL_SET}>No tools</SelectItem>
        {toolSets.map((toolSet) => (
          <SelectItem key={toolSet.id} value={toolSet.id}>
            {toolSet.name}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
