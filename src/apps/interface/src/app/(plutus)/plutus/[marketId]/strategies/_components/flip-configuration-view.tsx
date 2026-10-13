import { type FlipConfiguration } from "@/lib/api/plutus";
import { ConfigGroup } from "./config-group";
import { ConfigRow } from "./config-row";
import { FLIP_FIELDS } from "./flip-constants";
import { lockState } from "./parameter-locks";

export function FlipConfigurationView({
  configuration,
}: {
  configuration: FlipConfiguration;
}) {
  return (
    <ConfigGroup title="Trading Rules">
      <ConfigRow label="Mode" value={configuration.mode} />
      {FLIP_FIELDS.map((field) => (
        <ConfigRow
          key={field.key}
          label={field.label}
          value={configuration[field.key]}
          lock={
            field.isOptimized ? lockState(configuration, field.key) : "fixed"
          }
        />
      ))}
    </ConfigGroup>
  );
}
