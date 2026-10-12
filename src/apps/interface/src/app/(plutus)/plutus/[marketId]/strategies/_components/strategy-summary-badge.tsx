import { Badge } from "@/components/ui/badge";
import { type StrategyConfiguration } from "@/lib/api/plutus";

function describeConfiguration(configuration: StrategyConfiguration): string {
  if (configuration.mode === "Flip") {
    return `Flip · up to ${configuration.maxOpenOrders} open order${configuration.maxOpenOrders === 1 ? "" : "s"}`;
  }
  const activeInputCount = configuration.inputWeights.filter(
    (w) => w.weight !== 0,
  ).length;
  return `Swing · ${activeInputCount} active input${activeInputCount === 1 ? "" : "s"}`;
}

export function StrategySummaryBadge({
  configuration,
}: {
  configuration: StrategyConfiguration;
}) {
  return <Badge variant="secondary">{describeConfiguration(configuration)}</Badge>;
}
