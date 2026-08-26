import { NumericInput } from "@/components/shared/numeric-input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { type TradingConfiguration } from "@/lib/api/plutus";

type PositionLimitsCardProps = {
  tradingConfiguration: TradingConfiguration;
  onChange: (next: TradingConfiguration) => void;
};

export function PositionLimitsCard({
  tradingConfiguration,
  onChange,
}: PositionLimitsCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Position Limits</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <NumericInput
          label="Max Positions"
          hint="Maximum number of simultaneous positions"
          value={tradingConfiguration.maxPositions}
          onChange={(v) =>
            onChange({
              ...tradingConfiguration,
              maxPositions: v ?? 0,
            })
          }
          min={1}
          step={1}
        />
        <NumericInput
          label="Max Position Percent"
          hint="Max budget allocation per position (0-1)"
          value={tradingConfiguration.maxPositionPercent}
          onChange={(v) =>
            onChange({
              ...tradingConfiguration,
              maxPositionPercent: v ?? 0,
            })
          }
          min={0.01}
          max={1}
          step={0.01}
        />
        <NumericInput
          label="Hold Period Days"
          hint="Maximum days to hold a position"
          value={tradingConfiguration.holdPeriodDays}
          onChange={(v) =>
            onChange({
              ...tradingConfiguration,
              holdPeriodDays: v ?? 0,
            })
          }
          min={1}
          step={1}
        />
      </CardContent>
    </Card>
  );
}
