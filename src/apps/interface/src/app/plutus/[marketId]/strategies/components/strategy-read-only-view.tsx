import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { StrategyDetail } from "@/lib/api/plutus";
import { StrategyConfigurationView } from "./strategy-configuration-view";

export function StrategyReadOnlyView({ data }: { data: StrategyDetail }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Configuration</CardTitle>
      </CardHeader>
      <CardContent>
        <StrategyConfigurationView data={data} />
      </CardContent>
    </Card>
  );
}
