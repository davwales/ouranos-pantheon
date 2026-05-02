import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { StrategyDetail } from "@/lib/api/plutus";
import { StrategyConfigurationView } from "./strategy-configuration-view";

export function StrategyReadOnlyView({
  configuration,
}: {
  configuration: StrategyDetail["configuration"];
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Configuration</CardTitle>
      </CardHeader>
      <CardContent>
        <StrategyConfigurationView configuration={configuration} />
      </CardContent>
    </Card>
  );
}
