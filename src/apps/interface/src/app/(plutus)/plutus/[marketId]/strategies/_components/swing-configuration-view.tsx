import { type SwingConfiguration } from "@/lib/api/plutus";
import { ConfigGroup } from "./config-group";
import { ConfigRow } from "./config-row";
import { INPUT_KIND_LABELS, THRESHOLD_FIELDS } from "./strategy-constants";

export function SwingConfigurationView({
  configuration,
}: {
  configuration: SwingConfiguration;
}) {
  const activeWeights = configuration.inputWeights
    .filter((w) => w.weight !== 0)
    .sort((a, b) => b.weight - a.weight);

  const hasThresholds = THRESHOLD_FIELDS.some(
    (f) => configuration[f.key] != null,
  );

  return (
    <div className="space-y-4">
      <ConfigGroup title="Trading Rules">
        <ConfigRow label="Mode" value={configuration.mode} />
        <ConfigRow
          label="Max Positions"
          value={configuration.maxPositions ?? "Unlimited"}
        />
        <ConfigRow
          label="Max Position Percent"
          value={configuration.maxPositionPercent}
        />
        <ConfigRow
          label="Hold Period Days"
          value={configuration.holdPeriodDays ?? "Unlimited"}
        />
      </ConfigGroup>

      {activeWeights.length > 0 && (
        <ConfigGroup title="Input Weights">
          {activeWeights.map((w) => (
            <ConfigRow
              key={w.kind}
              label={INPUT_KIND_LABELS[w.kind]}
              value={w.weight}
            />
          ))}
        </ConfigGroup>
      )}

      {hasThresholds && (
        <ConfigGroup title="Thresholds">
          {THRESHOLD_FIELDS.map((f) => (
            <ConfigRow key={f.key} label={f.label} value={configuration[f.key]} />
          ))}
        </ConfigGroup>
      )}
    </div>
  );
}
