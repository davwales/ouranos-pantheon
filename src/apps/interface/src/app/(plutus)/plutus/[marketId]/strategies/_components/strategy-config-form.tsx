"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  type InputKind,
  type InputThresholds,
  type InputWeight,
  type StrategyDetail,
  type TradingConfiguration,
  plutusApi,
} from "@/lib/api/plutus";
import { RefreshCw } from "lucide-react";
import { useRouter } from "next/navigation";
import { useCallback, useState } from "react";
import {
  INPUT_KINDS,
  THRESHOLD_FIELDS,
} from "./strategy-constants";
import { StrategyBasicInfoCard } from "./strategy-basic-info-card";
import { PositionLimitsCard } from "./position-limits-card";
import { InputWeightsSection } from "./input-weights-section";
import { ThresholdsSection } from "./thresholds-section";

type ThresholdState = Record<keyof InputThresholds, number | null>;
type WeightState = Record<InputKind, number>;

const DEFAULT_TRADING_CONFIGURATION: TradingConfiguration = {
  maxPositions: 10,
  maxPositionPercent: 0.2,
  holdPeriodDays: 7,
};

function buildInitialWeights(strategy?: StrategyDetail): WeightState {
  const weights: WeightState = Object.fromEntries(
    INPUT_KINDS.map((kind) => [kind, 1]),
  ) as WeightState;
  if (strategy) {
    for (const inputWeight of strategy.inputWeights) {
      weights[inputWeight.kind] = inputWeight.weight;
    }
  }
  return weights;
}

function buildInitialThresholds(strategy?: StrategyDetail): ThresholdState {
  const t = strategy?.thresholds ?? {};
  return Object.fromEntries(
    THRESHOLD_FIELDS.map((field) => [field.key, t[field.key] ?? null]),
  ) as ThresholdState;
}

export function StrategyConfigForm({
  marketId,
  mode,
  strategyId,
  initialStrategy,
  onSuccess,
  onCancel,
}: {
  marketId: string;
  mode: "create" | "edit";
  strategyId?: string;
  initialStrategy?: StrategyDetail;
  onSuccess?: () => void;
  onCancel?: () => void;
}) {
  const router = useRouter();
  const isEdit = mode === "edit";

  const [name, setName] = useState(initialStrategy?.name ?? "");
  const [description, setDescription] = useState(
    initialStrategy?.description ?? "",
  );
  const [tradingConfiguration, setTradingConfiguration] =
    useState<TradingConfiguration>(
      initialStrategy?.tradingConfiguration ?? DEFAULT_TRADING_CONFIGURATION,
    );
  const [weights, setWeights] = useState<WeightState>(() =>
    buildInitialWeights(initialStrategy),
  );
  const [thresholds, setThresholds] = useState<ThresholdState>(() =>
    buildInitialThresholds(initialStrategy),
  );
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleWeightsChange = useCallback((next: WeightState) => {
    setWeights(next);
    setError(null);
  }, []);

  const handleSubmit = async (e?: React.FormEvent) => {
    e?.preventDefault();
    setIsProcessing(true);
    setError(null);
    try {
      const inputWeights: InputWeight[] = INPUT_KINDS.map((kind) => ({
        kind,
        weight: weights[kind],
      })).filter((w) => w.weight !== 0);

      if (inputWeights.length === 0) {
        setError("At least one input weight must be non-zero");
        return;
      }

      const cleanedThresholds: InputThresholds = {};
      for (const field of THRESHOLD_FIELDS) {
        const value = thresholds[field.key];
        if (value != null) {
          cleanedThresholds[field.key] = value;
        }
      }

      if (isEdit && strategyId) {
        await plutusApi.updateStrategy(strategyId, {
          name: name.trim(),
          description: description.trim() || null,
          tradingConfiguration,
          inputWeights,
          thresholds: cleanedThresholds,
        });
        onSuccess?.();
      } else {
        const response = await plutusApi.createStrategy({
          marketId,
          name: name.trim(),
          description: description.trim() || null,
          tradingConfiguration,
          inputWeights,
          thresholds: cleanedThresholds,
        });
        router.replace(`/plutus/${marketId}/strategies/${response.id}`);
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to save strategy",
      );
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <form className="space-y-6" onSubmit={handleSubmit}>
      {error && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
          {error}
        </div>
      )}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <StrategyBasicInfoCard
            name={name}
            description={description}
            onNameChange={setName}
            onDescriptionChange={setDescription}
          />

          <Card>
            <CardHeader>
              <CardTitle>Input Weights</CardTitle>
            </CardHeader>
            <CardContent>
              <InputWeightsSection weights={weights} onChange={handleWeightsChange} />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Thresholds</CardTitle>
            </CardHeader>
            <CardContent>
              <ThresholdsSection
                thresholds={thresholds}
                onChange={setThresholds}
              />
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <PositionLimitsCard
            tradingConfiguration={tradingConfiguration}
            onChange={setTradingConfiguration}
          />
        </div>
      </div>

      <div className="flex items-center justify-end gap-3">
        {isProcessing && <RefreshCw className="animate-spin size-4" />}
        {onCancel && (
          <Button type="button" variant="outline" onClick={onCancel} disabled={isProcessing}>
            Cancel
          </Button>
        )}
        <Button type="submit" disabled={isProcessing || !name.trim()}>
          {isProcessing
            ? isEdit
              ? "Saving..."
              : "Creating..."
            : isEdit
              ? "Save"
              : "Create"}
        </Button>
      </div>
    </form>
  );
}
