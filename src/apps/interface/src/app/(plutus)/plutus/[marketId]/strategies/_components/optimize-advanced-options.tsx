import { NumericInput } from "@/components/shared/numeric-input";

type OptimizeAdvancedOptionsProps = {
  sortinoWeight: number;
  onSortinoWeightChange: (value: number) => void;
  cagrWeight: number;
  onCagrWeightChange: (value: number) => void;
  drawdownWeight: number;
  onDrawdownWeightChange: (value: number) => void;
  turnoverWeight: number;
  onTurnoverWeightChange: (value: number) => void;
  l1RegularizationWeight: number;
  onL1RegularizationWeightChange: (value: number) => void;
  outSampleRatio: number;
  onOutSampleRatioChange: (value: number) => void;
  volumeParticipationRate: number;
  onVolumeParticipationRateChange: (value: number) => void;
  slippageMultiplier: number;
  onSlippageMultiplierChange: (value: number) => void;
};

export function OptimizeAdvancedOptions({
  sortinoWeight,
  onSortinoWeightChange,
  cagrWeight,
  onCagrWeightChange,
  drawdownWeight,
  onDrawdownWeightChange,
  turnoverWeight,
  onTurnoverWeightChange,
  l1RegularizationWeight,
  onL1RegularizationWeightChange,
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
        label="Sortino Weight"
        hint="Weight for downside-adjusted return"
        value={sortinoWeight}
        onChange={(v) => onSortinoWeightChange(v ?? 0)}
        step={0.1}
        min={-10}
        max={10}
      />
      <NumericInput
        label="CAGR Weight"
        hint="Weight for compound annual growth rate"
        value={cagrWeight}
        onChange={(v) => onCagrWeightChange(v ?? 0)}
        step={0.1}
        min={-10}
        max={10}
      />
      <NumericInput
        label="Drawdown Weight"
        hint="Weight penalizing max drawdown"
        value={drawdownWeight}
        onChange={(v) => onDrawdownWeightChange(v ?? 0)}
        step={0.1}
        min={-10}
        max={10}
      />
      <NumericInput
        label="Turnover Weight"
        hint="Weight penalizing excessive trading"
        value={turnoverWeight}
        onChange={(v) => onTurnoverWeightChange(v ?? 0)}
        step={0.1}
        min={-10}
        max={10}
      />
      <NumericInput
        label="L1 Regularization Weight"
        hint="Sparsity penalty on input weights"
        value={l1RegularizationWeight}
        onChange={(v) => onL1RegularizationWeightChange(v ?? 0)}
        step={0.05}
        min={0}
        max={10}
      />
      <NumericInput
        label="Out-of-Sample Ratio"
        hint="Fraction of data reserved for walk-forward validation"
        value={outSampleRatio}
        onChange={(v) => onOutSampleRatioChange(v ?? 0.2)}
        step={0.05}
        min={0}
        max={1}
      />
      <p className="text-xs text-muted-foreground">
        The out-of-sample ratio controls the walk-forward validation split:
        this fraction of the date range is held out and used to validate the
        optimized parameters against unseen data.
      </p>
      <NumericInput
        label="Volume Participation Rate"
        hint="Max fraction of daily volume per trade (0-1)"
        value={volumeParticipationRate}
        onChange={(v) => onVolumeParticipationRateChange(v ?? 0.25)}
        min={0.01}
        max={1}
        step={0.01}
      />
      <NumericInput
        label="Slippage Multiplier"
        hint="Price impact per unit of volume ratio (0 = none)"
        value={slippageMultiplier}
        onChange={(v) => onSlippageMultiplierChange(v ?? 0.1)}
        min={0}
        max={1}
        step={0.01}
      />
    </div>
  );
}
