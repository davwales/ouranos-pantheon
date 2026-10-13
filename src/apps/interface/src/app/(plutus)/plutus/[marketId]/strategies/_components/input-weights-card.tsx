import { NumericInput } from "@/components/shared/numeric-input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { type InputKind, type InputWeight } from "@/lib/api/plutus";
import { LockToggle } from "./lock-toggle";
import { INPUT_KIND_LABELS } from "./strategy-constants";

type InputWeightsCardProps = {
  weights: InputWeight[];
  onChange: (next: InputWeight[]) => void;
  isWeightLocked: (kind: InputKind) => boolean;
  onToggleLock: (kind: InputKind) => void;
};

export function InputWeightsCard({
  weights,
  onChange,
  isWeightLocked,
  onToggleLock,
}: InputWeightsCardProps) {
  const handleChange = (kind: InputKind, weight: number) => {
    onChange(weights.map((w) => (w.kind === kind ? { kind, weight } : w)));
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Input Weights</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-2">
          {weights.map(({ kind, weight }) => {
            const id = `weight-${kind}`;
            return (
              <div key={kind} className="flex items-center gap-4">
                <label htmlFor={id} className="text-sm flex-1 min-w-0 truncate">
                  {INPUT_KIND_LABELS[kind]}
                </label>
                <LockToggle
                  label={`${INPUT_KIND_LABELS[kind]} weight`}
                  locked={weight !== 0 && isWeightLocked(kind)}
                  disabledReason={
                    weight === 0 ? "Set a non-zero weight to lock it" : undefined
                  }
                  onToggle={() => onToggleLock(kind)}
                />
                <NumericInput
                  id={id}
                  value={weight}
                  onChange={(v) => handleChange(kind, v ?? 0)}
                  min={0}
                  step={0.1}
                  className="w-28"
                  aria-label={INPUT_KIND_LABELS[kind]}
                />
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
