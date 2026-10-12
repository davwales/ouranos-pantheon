import { NumericInput } from "@/components/shared/numeric-input";
import {
  DEFAULT_MIN_TRADES_PER_MONTH,
  DEFAULT_OUT_SAMPLE_RATIO,
  DEFAULT_SLIPPAGE_MULTIPLIER,
  DEFAULT_VOLUME_PARTICIPATION_RATE,
} from "./execution-constants";

type OptimizeAdvancedOptionsProps = {
  drawdownWeight: number;
  onDrawdownWeightChange: (value: number) => void;
  minTradesPerMonth: number;
  onMinTradesPerMonthChange: (value: number) => void;
  outSampleRatio: number;
  onOutSampleRatioChange: (value: number) => void;
  volumeParticipationRate: number;
  onVolumeParticipationRateChange: (value: number) => void;
  slippageMultiplier: number;
  onSlippageMultiplierChange: (value: number) => void;
};

export function OptimizeAdvancedOptions({
  drawdownWeight,
  onDrawdownWeightChange,
  minTradesPerMonth,
  onMinTradesPerMonthChange,
  outSampleRatio,
  onOutSampleRatioChange,
  volumeParticipationRate,
  onVolumeParticipationRateChange,
  slippageMultiplier,
  onSlippageMultiplierChange,
}: OptimizeAdvancedOptionsProps) {
  return (
    <div className="space-y-4 border rounded-lg p-3 bg-muted/30">
      <NumericInput
        label="Drawdown Weight"
        hint="Fitness penalty per unit of max drawdown"
        value={drawdownWeight}
        onChange={(v) => onDrawdownWeightChange(v ?? 0)}
        step={0.1}
        min={0}
        max={10}
      />
      <NumericInput
        label="Min Trades per Month"
        hint="Round trips a candidate must make in each ~30-day segment before it is penalized"
        value={minTradesPerMonth}
        onChange={(v) => onMinTradesPerMonthChange(v ?? DEFAULT_MIN_TRADES_PER_MONTH)}
        step={1}
        min={0}
        max={500}
      />
      <NumericInput
        label="Out-of-Sample Ratio"
        hint="Fraction of the date range held out to validate the result"
        value={outSampleRatio}
        onChange={(v) => onOutSampleRatioChange(v ?? DEFAULT_OUT_SAMPLE_RATIO)}
        step={0.05}
        min={0}
        max={1}
      />
      <p className="text-xs text-muted-foreground">
        Candidates are scored on every ~30-day segment of the in-sample range,
        so a strategy has to hold up across the whole period. The held-out
        tail then checks the result on unseen data; it is validated only when
        both periods are profitable.
      </p>
      <NumericInput
        label="Volume Participation Rate"
        hint="Max fraction of an item's typical daily volume per fill (0-1)"
        value={volumeParticipationRate}
        onChange={(v) =>
          onVolumeParticipationRateChange(v ?? DEFAULT_VOLUME_PARTICIPATION_RATE)
        }
        min={0.01}
        max={1}
        step={0.01}
      />
      <NumericInput
        label="Slippage Multiplier"
        hint="Square-root price impact coefficient (0 = fill at the daily average)"
        value={slippageMultiplier}
        onChange={(v) => onSlippageMultiplierChange(v ?? DEFAULT_SLIPPAGE_MULTIPLIER)}
        min={0}
        max={1}
        step={0.01}
      />
    </div>
  );
}
