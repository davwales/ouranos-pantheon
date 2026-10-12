import { type SwingConfiguration } from "@/lib/api/plutus";
import { InputWeightsCard } from "./input-weights-card";
import { PositionLimitsCard } from "./position-limits-card";
import { ThresholdsCard } from "./thresholds-card";

type SwingConfigFormProps = {
  configuration: SwingConfiguration;
  onChange: (next: SwingConfiguration) => void;
};

export function SwingConfigForm({ configuration, onChange }: SwingConfigFormProps) {
  return (
    <div className="space-y-6">
      <InputWeightsCard
        weights={configuration.inputWeights}
        onChange={(inputWeights) => onChange({ ...configuration, inputWeights })}
      />
      <ThresholdsCard configuration={configuration} onChange={onChange} />
      <PositionLimitsCard configuration={configuration} onChange={onChange} />
    </div>
  );
}
