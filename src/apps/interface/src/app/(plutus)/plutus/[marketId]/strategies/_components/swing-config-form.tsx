import { type SwingConfiguration } from "@/lib/api/plutus";
import { InputWeightsCard } from "./input-weights-card";
import { isLocked, weightLockKey, withLock } from "./parameter-locks";
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
        isWeightLocked={(kind) => isLocked(configuration, weightLockKey(kind))}
        onToggleLock={(kind) =>
          onChange(
            withLock(
              configuration,
              weightLockKey(kind),
              !isLocked(configuration, weightLockKey(kind)),
            ),
          )
        }
      />
      <ThresholdsCard configuration={configuration} onChange={onChange} />
      <PositionLimitsCard configuration={configuration} onChange={onChange} />
    </div>
  );
}
