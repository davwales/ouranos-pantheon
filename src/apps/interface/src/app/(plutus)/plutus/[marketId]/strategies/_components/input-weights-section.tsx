import { NumericInput } from "@/components/shared/numeric-input";
import { type InputKind } from "@/lib/api/plutus";
import { INPUT_KINDS, INPUT_KIND_LABELS } from "./strategy-constants";

type WeightState = Record<InputKind, number>;

type InputWeightsSectionProps = {
  weights: WeightState;
  onChange: (next: WeightState) => void;
};

export function InputWeightsSection({
  weights,
  onChange,
}: InputWeightsSectionProps) {
  return (
    <div className="space-y-2">
      {INPUT_KINDS.map((kind) => {
        const id = `weight-${kind}`;
        return (
          <div key={kind} className="flex items-center gap-4">
            <label
              htmlFor={id}
              className="text-sm flex-1 min-w-0 truncate"
            >
              {INPUT_KIND_LABELS[kind]}
            </label>
            <NumericInput
              id={id}
              value={weights[kind]}
              onChange={(v) => onChange({ ...weights, [kind]: v ?? 0 })}
              min={0}
              step={0.1}
              className="w-28"
              aria-label={INPUT_KIND_LABELS[kind]}
            />
          </div>
        );
      })}
    </div>
  );
}
