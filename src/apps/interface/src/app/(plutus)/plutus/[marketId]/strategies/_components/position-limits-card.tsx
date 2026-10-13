import { NumericInput } from "@/components/shared/numeric-input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { type SwingConfiguration } from "@/lib/api/plutus";
import { SwingLockToggle } from "./swing-lock-toggle";

type PositionLimitsCardProps = {
  configuration: SwingConfiguration;
  onChange: (next: SwingConfiguration) => void;
};

export function PositionLimitsCard({
  configuration,
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
          labelAction={
            <SwingLockToggle
              configuration={configuration}
              fieldKey="maxPositions"
              label="Max Positions"
              onChange={onChange}
            />
          }
          hint="Maximum number of simultaneous positions"
          placeholder="Unlimited"
          value={configuration.maxPositions}
          onChange={(v) => onChange({ ...configuration, maxPositions: v })}
          min={1}
          step={1}
        />
        <NumericInput
          label="Max Position Percent"
          labelAction={
            <SwingLockToggle
              configuration={configuration}
              fieldKey="maxPositionPercent"
              label="Max Position Percent"
              onChange={onChange}
            />
          }
          hint="Max budget allocation per position (0-1)"
          value={configuration.maxPositionPercent}
          onChange={(v) =>
            onChange({ ...configuration, maxPositionPercent: v })
          }
          min={0.01}
          max={1}
          step={0.01}
        />
        <NumericInput
          label="Hold Period Days"
          labelAction={
            <SwingLockToggle
              configuration={configuration}
              fieldKey="holdPeriodDays"
              label="Hold Period Days"
              onChange={onChange}
            />
          }
          hint="Maximum days to hold a position"
          placeholder="Unlimited"
          value={configuration.holdPeriodDays}
          onChange={(v) => onChange({ ...configuration, holdPeriodDays: v })}
          min={1}
          step={1}
        />
      </CardContent>
    </Card>
  );
}
