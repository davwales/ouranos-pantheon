import { NumericInput } from "@/components/shared/numeric-input";
import { type InputThresholds } from "@/lib/api/plutus";
import { THRESHOLD_FIELDS } from "./strategy-constants";

type ThresholdState = Record<keyof InputThresholds, number | null>;

type ThresholdsSectionProps = {
  thresholds: ThresholdState;
  onChange: (next: ThresholdState) => void;
};

export function ThresholdsSection({
  thresholds,
  onChange,
}: ThresholdsSectionProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
      {THRESHOLD_FIELDS.map((field) => (
        <NumericInput
          key={field.key}
          id={`threshold-${field.key}`}
          label={field.label}
          hint={field.hint}
          placeholder={field.placeholder}
          value={thresholds[field.key]}
          onChange={(v) => onChange({ ...thresholds, [field.key]: v })}
          min={field.min}
          max={field.max}
          step={field.step}
          aria-label={field.label}
        />
      ))}
    </div>
  );
}
