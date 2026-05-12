"use client";

import { NumericInput } from "@/app/components/numeric-input";
import { Input } from "@/components/ui/input";
import { type StrategyConfigBundle, type StrategyType } from "@/lib/api/plutus";
import { signalWeightFields, type SignalWeightKey } from "./strategy-constants";

function SignalWeightedConfigSection({
  config,
  onChange,
}: {
  config: NonNullable<StrategyConfigBundle["signalWeightedConfig"]>;
  onChange: (c: StrategyConfigBundle["signalWeightedConfig"]) => void;
}) {
  function handleWeightChange(key: SignalWeightKey, value: number) {
    onChange({ ...config, [key]: value });
  }

  return (
    <div className="space-y-4">
      <h4 className="text-sm font-semibold">Signal Weights</h4>
      <div className="space-y-2">
        {signalWeightFields.map((field) => (
          <div key={field.key} className="flex items-center gap-4">
            <span className="text-sm flex-1 min-w-0 truncate">
              {field.label as string}
            </span>
            <Input
              type="number"
              value={config[field.key] ?? 1}
              onChange={(e) =>
                handleWeightChange(field.key, parseFloat(e.target.value) || 0)
              }
              className="w-24"
              step={0.1}
              min={0}
            />
          </div>
        ))}
      </div>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <NumericInput
          label="Buy Threshold"
          hint="Score above which to buy"
          value={config.buyThreshold}
          onChange={(v) => onChange({ ...config, buyThreshold: v })}
          min={0}
          max={1}
          step={0.01}
        />
        <NumericInput
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

function ForecastMomentumConfigSection({
  config,
  onChange,
}: {
  config: NonNullable<StrategyConfigBundle["forecastMomentumConfig"]>;
  onChange: (c: StrategyConfigBundle["forecastMomentumConfig"]) => void;
}) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <NumericInput
        label="Forecast Movement Threshold"
        hint="Min price change to trigger a signal"
        value={config.forecastMovementThreshold}
        onChange={(v) => onChange({ ...config, forecastMovementThreshold: v })}
        min={0}
        step={0.01}
      />
      <NumericInput
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

function MeanReversionConfigSection({
  config,
  onChange,
}: {
  config: NonNullable<StrategyConfigBundle["meanReversionConfig"]>;
  onChange: (c: StrategyConfigBundle["meanReversionConfig"]) => void;
}) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <NumericInput
        label="Deviation Multiplier"
        hint="Standard deviations from mean to trigger"
        value={config.deviationMultiplier}
        onChange={(v) => onChange({ ...config, deviationMultiplier: v })}
        min={0.1}
        step={0.1}
      />
      <NumericInput
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

function RecipeArbitrageConfigSection({
  config,
  onChange,
}: {
  config: NonNullable<StrategyConfigBundle["recipeArbitrageConfig"]>;
  onChange: (c: StrategyConfigBundle["recipeArbitrageConfig"]) => void;
}) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <NumericInput
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
  bundle,
  onChange,
}: {
  type: StrategyType;
  bundle: StrategyConfigBundle;
  onChange: (bundle: StrategyConfigBundle) => void;
}) {
  return (
    <div className="space-y-4">
      {type === "SignalWeighted" && (
        <SignalWeightedConfigSection
          config={bundle.signalWeightedConfig ?? {}}
          onChange={(c) => onChange({ ...bundle, signalWeightedConfig: c })}
        />
      )}
      {type === "ForecastMomentum" && (
        <ForecastMomentumConfigSection
          config={bundle.forecastMomentumConfig ?? {}}
          onChange={(c) => onChange({ ...bundle, forecastMomentumConfig: c })}
        />
      )}
      {type === "MeanReversion" && (
        <MeanReversionConfigSection
          config={bundle.meanReversionConfig ?? {}}
          onChange={(c) => onChange({ ...bundle, meanReversionConfig: c })}
        />
      )}
      {type === "RecipeArbitrage" && (
        <RecipeArbitrageConfigSection
          config={bundle.recipeArbitrageConfig ?? {}}
          onChange={(c) => onChange({ ...bundle, recipeArbitrageConfig: c })}
        />
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
