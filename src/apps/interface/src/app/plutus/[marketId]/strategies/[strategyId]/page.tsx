"use client";

import { ConfirmationButton } from "@/app/components/confirmation-button";
import { Typography } from "@/app/components/typography";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useApi } from "@/hooks/use-api";
import {
  type StrategyConfiguration,
  type StrategyDetail,
  type StrategyType,
  plutusApi,
} from "@/lib/api/plutus";
import {
  Activity,
  Calendar,
  Clock,
  Gauge,
  Pencil,
  RefreshCw,
  Trash2,
  X,
} from "lucide-react";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { NumberInput } from "../components/number-input";
import { StrategyConfigForm } from "../components/strategy-config-form";

const strategyTypeLabels: Record<StrategyType, string> = {
  SignalWeighted: "Signal Weighted",
  ForecastMomentum: "Forecast Momentum",
  MeanReversion: "Mean Reversion",
  RecipeArbitrage: "Recipe Arbitrage",
  Composite: "Composite",
};

const signalTypeLabels: Record<string, string> = {
  TaxAdjustedRoi: "Tax Adjusted ROI",
  VolumeAnomaly: "Volume Anomaly",
  TrendMomentum: "Trend Momentum",
  BollingerBands: "Bollinger Bands",
  Rsi: "RSI",
  MovingAverageCrossover: "Moving Average Crossover",
  PriceVelocity: "Price Velocity",
};

function FieldLabel({
  children,
  hint,
}: {
  children: React.ReactNode;
  hint?: string;
}) {
  return (
    <label className="text-sm font-medium block">
      {children}
      {hint && (
        <span className="text-muted-foreground text-xs ml-1">({hint})</span>
      )}
    </label>
  );
}

function InfoChip({ label, value }: { label: string; value: string }) {
  return (
    <span className="inline-flex items-center gap-1 rounded-full border bg-muted/50 px-2.5 py-1 text-xs font-medium">
      <span className="text-muted-foreground">{label}:</span>
      {value}
    </span>
  );
}

function ConfigRow({
  label,
  value,
}: {
  label: string;
  value: string | number | null | undefined;
}) {
  if (value == null) return null;
  return (
    <div className="flex justify-between items-center py-2 border-b border-border/50 last:border-b-0">
      <span className="text-sm text-muted-foreground">{label}</span>
      <span className="text-sm font-medium">{String(value)}</span>
    </div>
  );
}

function typeIcon(type: StrategyType) {
  switch (type) {
    case "SignalWeighted":
      return <Gauge className="size-5" />;
    case "ForecastMomentum":
      return <Activity className="size-5" />;
    case "MeanReversion":
      return <Clock className="size-5" />;
    case "RecipeArbitrage":
      return <Calendar className="size-5" />;
    default:
      return <Gauge className="size-5" />;
  }
}

function StrategyConfigurationView({
  configuration,
}: {
  configuration: StrategyDetail["configuration"];
}) {
  const signalTypes = [
    "TaxAdjustedRoi",
    "VolumeAnomaly",
    "TrendMomentum",
    "BollingerBands",
    "Rsi",
    "MovingAverageCrossover",
    "PriceVelocity",
  ];

  const weights =
    configuration.signalWeights && configuration.signalWeights.length > 0
      ? configuration.signalWeights
      : signalTypes.map((t) => ({ type: t, weight: 1 }));

  return (
    <div className="space-y-4">
      {weights.length > 0 && (
        <div className="space-y-2">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Signal Weights
          </Typography>
          {weights.map((w, i) => (
            <ConfigRow
              key={i}
              label={signalTypeLabels[w.type] ?? w.type}
              value={w.weight}
            />
          ))}
        </div>
      )}
      <div>
        <Typography
          variant="small"
          className="font-semibold uppercase tracking-wide text-muted-foreground"
        >
          Parameters
        </Typography>
        <div className="mt-2 space-y-1">
          <ConfigRow label="Buy Threshold" value={configuration.buyThreshold} />
          <ConfigRow
            label="Sell Threshold"
            value={configuration.sellThreshold}
          />
          <ConfigRow
            label="Forecast Movement Threshold"
            value={configuration.forecastMovementThreshold}
          />
          <ConfigRow
            label="Forecast Horizon Days"
            value={configuration.forecastHorizonDays}
          />
          <ConfigRow
            label="Deviation Multiplier"
            value={configuration.deviationMultiplier}
          />
          <ConfigRow
            label="Mean Time Frame Value"
            value={configuration.meanTimeFrameValue}
          />
          <ConfigRow
            label="Min Margin Percent"
            value={configuration.minMarginPercent}
          />
          <ConfigRow label="Max Positions" value={configuration.maxPositions} />
          <ConfigRow
            label="Max Position Percent"
            value={configuration.maxPositionPercent}
          />
          <ConfigRow
            label="Hold Period Days"
            value={configuration.holdPeriodDays}
          />
        </div>
      </div>
      {configuration.components && configuration.components.length > 0 && (
        <div className="space-y-2">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Components
          </Typography>
          {configuration.components.map((c, i) => (
            <ConfigRow
              key={i}
              label={strategyTypeLabels[c.type] ?? c.type}
              value={`Weight: ${c.weight}`}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default function StrategyDetailPage() {
  const { marketId, strategyId } = useParams<{
    marketId: string;
    strategyId: string;
  }>();
  const router = useRouter();

  const [strategy, reexecute] = useApi<StrategyDetail>(
    () => plutusApi.getStrategy(strategyId),
    [strategyId],
  );

  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [toggling, setToggling] = useState(false);

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [config, setConfig] = useState<StrategyConfiguration>({});

  const data = strategy.data;

  useEffect(() => {
    if (data) {
      setName(data.name);
      setDescription(data.description ?? "");
      setConfig({ ...data.configuration });
    }
  }, [data]);

  const handleToggleActive = async () => {
    if (!data) return;
    setToggling(true);
    try {
      await plutusApi.setStrategyActive(strategyId, !data.isActive);
      reexecute();
    } catch (err) {
      console.error("Failed to toggle active status", err);
    } finally {
      setToggling(false);
    }
  };

  const handleDelete = async () => {
    try {
      await plutusApi.deleteStrategy(strategyId);
      router.replace(`/plutus/${marketId}/strategies`);
    } catch (err) {
      console.error("Failed to delete strategy", err);
    }
  };

  const handleSave = async () => {
    if (!data) return;
    setIsSaving(true);
    try {
      await plutusApi.updateStrategy(strategyId, {
        name: name.trim(),
        description: description.trim() || null,
        configuration: config,
      });
      reexecute();
      setIsEditing(false);
    } catch (err) {
      console.error("Failed to save strategy", err);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    if (data) {
      setName(data.name);
      setDescription(data.description ?? "");
      setConfig({ ...data.configuration });
    }
    setIsEditing(false);
  };

  if (strategy.status === "error") {
    return <Typography variant="lead">Error loading strategy</Typography>;
  }

  if (!data) {
    return (
      <div className="flex items-center justify-center py-8">
        <RefreshCw className="animate-spin" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <Card className="border-l-4">
        <CardContent className="pt-6 pb-6">
          <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
            <div className="space-y-3 min-w-0">
              <div className="flex items-center gap-3">
                <div className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary shrink-0">
                  {typeIcon(data.type)}
                </div>
                <div className="min-w-0">
                  <h2 className="text-2xl font-semibold tracking-tight truncate">
                    {data.name}
                  </h2>
                  <p className="text-sm text-muted-foreground">
                    {strategyTypeLabels[data.type]} ·{" "}
                    <span
                      className={
                        data.isActive
                          ? "text-green-600 dark:text-green-400 font-medium"
                          : "text-red-600 dark:text-red-400 font-medium"
                      }
                    >
                      {data.isActive ? "Active" : "Inactive"}
                    </span>
                  </p>
                </div>
              </div>

              {data.description && (
                <p className="text-muted-foreground text-sm">
                  {data.description}
                </p>
              )}

              <div className="flex flex-wrap gap-2">
                <InfoChip
                  label="Created"
                  value={new Date(data.createdAt).toLocaleDateString()}
                />
                <InfoChip
                  label="Updated"
                  value={new Date(data.updatedAt).toLocaleDateString()}
                />
              </div>
            </div>

            <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 w-full sm:w-auto">
              {isEditing ? (
                <>
                  <Button
                    className="w-full sm:w-auto"
                    variant="outline"
                    size="sm"
                    onClick={handleCancel}
                    disabled={isSaving}
                  >
                    <X className="size-4 mr-1" />
                    Cancel
                  </Button>
                  <Button
                    className="w-full sm:w-auto"
                    size="sm"
                    onClick={handleSave}
                    disabled={isSaving || !name.trim()}
                  >
                    Save
                  </Button>
                </>
              ) : (
                <>
                  <Button
                    className="w-full sm:w-auto"
                    variant="ghost"
                    size="sm"
                    onClick={() => setIsEditing(true)}
                  >
                    <Pencil className="size-4 mr-1" />
                    Edit
                  </Button>
                  <Button
                    className="w-full sm:w-auto"
                    onClick={handleToggleActive}
                    disabled={toggling}
                    variant={data.isActive ? "destructive" : "default"}
                    size="sm"
                  >
                    {toggling
                      ? "..."
                      : data.isActive
                        ? "Deactivate"
                        : "Activate"}
                  </Button>
                  <ConfirmationButton
                    className="w-full sm:w-auto"
                    title="Delete Strategy"
                    description="Are you sure you want to delete this strategy? This action cannot be undone."
                    onConfirm={handleDelete}
                    variant="outline"
                    size="sm"
                  >
                    <Trash2 className="size-4 mr-1" />
                    Delete
                  </ConfirmationButton>
                </>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Body */}
      {isEditing ? (
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
                    onChange={(e) => setName(e.target.value)}
                    placeholder="e.g. Aggressive Signal Strategy"
                  />
                </div>
                <div className="space-y-1">
                  <FieldLabel>Description (optional)</FieldLabel>
                  <Textarea
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
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
                  config={config}
                  onChange={setConfig}
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
                <NumberInput
                  label="Max Positions"
                  hint="Maximum number of simultaneous positions"
                  value={config.maxPositions}
                  onChange={(v) => setConfig({ ...config, maxPositions: v })}
                  min={1}
                  step={1}
                />
                <NumberInput
                  label="Max Position Percent"
                  hint="Max budget allocation per position (0-1)"
                  value={config.maxPositionPercent}
                  onChange={(v) =>
                    setConfig({ ...config, maxPositionPercent: v })
                  }
                  min={0.01}
                  max={1}
                  step={0.01}
                />
                <NumberInput
                  label="Hold Period Days"
                  hint="Maximum days to hold a position"
                  value={config.holdPeriodDays}
                  onChange={(v) => setConfig({ ...config, holdPeriodDays: v })}
                  min={1}
                  step={1}
                />
              </CardContent>
            </Card>
          </div>
        </div>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>Configuration</CardTitle>
          </CardHeader>
          <CardContent>
            <StrategyConfigurationView configuration={data.configuration} />
          </CardContent>
        </Card>
      )}
    </div>
  );
}
