import { type StrategyConfiguration } from "@/lib/api/plutus";
import { FlipConfigurationView } from "./flip-configuration-view";
import { SwingConfigurationView } from "./swing-configuration-view";

export function StrategyConfigurationView({
  configuration,
}: {
  configuration: StrategyConfiguration;
}) {
  return configuration.mode === "Swing" ? (
    <SwingConfigurationView configuration={configuration} />
  ) : (
    <FlipConfigurationView configuration={configuration} />
  );
}
