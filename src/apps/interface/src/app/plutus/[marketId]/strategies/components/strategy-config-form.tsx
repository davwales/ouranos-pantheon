"use client";

import { Input } from "@/components/ui/input";
import {
  type StrategyConfiguration,
  type StrategyType,
} from "@/lib/api/plutus";
import { NumberInput } from "./number-input";
import { signalTypeLabels } from "./strategy-constants";

function SignalWeightedConfig({
  config,
  onChange,
}: {
  config: StrategyConfiguration;
  onChange: (c: StrategyConfiguration) => void;
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
    config.signalWeights && config.signalWeights.length > 0
      ? config.signalWeights
      : signalTypes.map((t) => ({ type: t, weight: 1 }));

  return (
    <div className="space-y-4">
      <h4 className="text-sm font-semibold">Signal Weights</h4>
      <div className="space-y-2">
        {weights.map((w, i) => (
          <div key={w.type} className="flex items-center gap-4">
            <span className="text-sm flex-1 min-w-0 truncate">
              {signalTypeLabels[w.type] ?? w.type}
            </span>
            <Input
              type="number"
              value={w.weight}
              onChange={(e) => {
                const newWeights = [...weights];
                newWeights[i] = {
                  ...newWeights[i],
                  weight: parseFloat(e.target.value) || 0,
                };
                onChange({ ...config, signalWeights: newWeights });
              }}
              className="w-24"
              step={0.1}
              min={0}
            />
          </div>
        ))}
      </div>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <NumberInput
          label="Buy Threshold"
          hint="Score above which to buy"
          value={config.buyThreshold}
          onChange={(v) => onChange({ ...config, buyThreshold: v })}
          min={0}
          max={1}
          step={0.01}
        />
        <NumberInput
          label="Sell Threshold"
          hint="Score below which to sell (negative)"
          value={config.sellThreshold}
          onChange={(v) => onChange({ ...config, sellThreshold: v })}
          min={-1}
          max={0}
          step={0.01}
        />
      </div>
    </div>
  );
}

function ForecastMomentumConfig({
  config,
  onChange,
}: {
  config: StrategyConfiguration;
  onChange: (c: StrategyConfiguration) => void;
}) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <NumberInput
        label="Forecast Movement Threshold"
        hint="Min price change to trigger a signal"
        value={config.forecastMovementThreshold}
        onChange={(v) => onChange({ ...config, forecastMovementThreshold: v })}
        min={0}
        step={0.01}
      />
      <NumberInput
        label="Forecast Horizon Days"
        value={config.forecastHorizonDays}
        onChange={(v) => onChange({ ...config, forecastHorizonDays: v })}
        min={1}
        max={30}
        step={1}
      />
    </div>
  );
}

function MeanReversionConfig({
  config,
  onChange,
}: {
  config: StrategyConfiguration;
  onChange: (c: StrategyConfiguration) => void;
}) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <NumberInput
        label="Deviation Multiplier"
        hint="Standard deviations from mean to trigger"
        value={config.deviationMultiplier}
        onChange={(v) => onChange({ ...config, deviationMultiplier: v })}
        min={0.1}
        step={0.1}
      />
      <NumberInput
        label="Mean Time Frame Value"
        hint="Time frame for mean calculation"
        value={config.meanTimeFrameValue}
        onChange={(v) => onChange({ ...config, meanTimeFrameValue: v })}
        min={1}
        max={4}
        step={1}
      />
    </div>
  );
}

function RecipeArbitrageConfig({
  config,
  onChange,
}: {
  config: StrategyConfiguration;
  onChange: (c: StrategyConfiguration) => void;
}) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <NumberInput
        label="Min Margin Percent"
        hint="Minimum margin % to consider a recipe profitable"
        value={config.minMarginPercent}
        onChange={(v) => onChange({ ...config, minMarginPercent: v })}
        min={0}
        max={1}
        step={0.01}
      />
    </div>
  );
}

export function StrategyConfigForm({
  type,
  config,
  onChange,
}: {
  type: StrategyType;
  config: StrategyConfiguration;
  onChange: (c: StrategyConfiguration) => void;
}) {
  return (
    <div className="space-y-4">
      {type === "SignalWeighted" && (
        <SignalWeightedConfig config={config} onChange={onChange} />
      )}
      {type === "ForecastMomentum" && (
        <ForecastMomentumConfig config={config} onChange={onChange} />
      )}
      {type === "MeanReversion" && (
        <MeanReversionConfig config={config} onChange={onChange} />
      )}
      {type === "RecipeArbitrage" && (
        <RecipeArbitrageConfig config={config} onChange={onChange} />
      )}
      {type === "Composite" && (
        <p className="text-sm text-muted-foreground">
          Composite strategies are managed through their component strategies.
          Configuration is determined by the weighted components.
        </p>
      )}
    </div>
  );
}
