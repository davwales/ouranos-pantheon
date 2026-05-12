"use client";

import { NumericInput } from "@/app/components/numeric-input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { StrategyConfigBundle, StrategyDetail } from "@/lib/api/plutus";
import { FieldLabel } from "./field-label";
import { StrategyConfigForm } from "./strategy-config-form";
import { strategyTypeLabels } from "./strategy-constants";

export function StrategyEditForm({
  data,
  name,
  description,
  bundle,
  onNameChange,
  onDescriptionChange,
  onBundleChange,
}: {
  data: StrategyDetail;
  name: string;
  description: string;
  bundle: StrategyConfigBundle;
  onNameChange: (v: string) => void;
  onDescriptionChange: (v: string) => void;
  onBundleChange: (v: StrategyConfigBundle) => void;
}) {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div className="lg:col-span-2 space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Basic Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-1">
              <FieldLabel>Name</FieldLabel>
              <Input
                value={name}
                onChange={(e) => onNameChange(e.target.value)}
                placeholder="e.g. Aggressive Signal Strategy"
              />
            </div>
            <div className="space-y-1">
              <FieldLabel>Description (optional)</FieldLabel>
              <Textarea
                value={description}
                onChange={(e) => onDescriptionChange(e.target.value)}
                placeholder="Describe the strategy's goals and parameters"
                rows={3}
              />
            </div>
            <div className="space-y-1">
              <FieldLabel>Type</FieldLabel>
              <p className="text-sm text-muted-foreground">
                {strategyTypeLabels[data.type]} (cannot be changed)
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Configuration</CardTitle>
          </CardHeader>
          <CardContent>
            <StrategyConfigForm
              type={data.type}
              bundle={bundle}
              onChange={onBundleChange}
            />
          </CardContent>
        </Card>
      </div>

      <div className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Position Limits</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <NumericInput
              label="Max Positions"
              hint="Maximum number of simultaneous positions"
              value={bundle.tradingConfiguration.maxPositions}
              onChange={(v) =>
                onBundleChange({
                  ...bundle,
                  tradingConfiguration: {
                    ...bundle.tradingConfiguration,
                    maxPositions: v ?? 0,
                  },
                })
              }
              min={1}
              step={1}
            />
            <NumericInput
              label="Max Position Percent"
              hint="Max budget allocation per position (0-1)"
              value={bundle.tradingConfiguration.maxPositionPercent}
              onChange={(v) =>
                onBundleChange({
                  ...bundle,
                  tradingConfiguration: {
                    ...bundle.tradingConfiguration,
                    maxPositionPercent: v ?? 0,
                  },
                })
              }
              min={0.01}
              max={1}
              step={0.01}
            />
            <NumericInput
              label="Hold Period Days"
              hint="Maximum days to hold a position"
              value={bundle.tradingConfiguration.holdPeriodDays}
              onChange={(v) =>
                onBundleChange({
                  ...bundle,
                  tradingConfiguration: {
                    ...bundle.tradingConfiguration,
                    holdPeriodDays: v ?? 0,
                  },
                })
              }
              min={1}
              step={1}
            />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
