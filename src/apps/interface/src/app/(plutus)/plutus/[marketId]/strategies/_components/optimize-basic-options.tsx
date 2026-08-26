import { NumericInput } from "@/components/shared/numeric-input";
import { Input } from "@/components/ui/input";

type OptimizeBasicOptionsProps = {
  startDate: string;
  onStartDateChange: (value: string) => void;
  endDate: string;
  onEndDateChange: (value: string) => void;
  budget: number;
  onBudgetChange: (value: number) => void;
  generations: number;
  onGenerationsChange: (value: number) => void;
  populationSize: number;
  onPopulationSizeChange: (value: number) => void;
  dateInvalid: boolean;
};

export function OptimizeBasicOptions({
  startDate,
  onStartDateChange,
  endDate,
  onEndDateChange,
  budget,
  onBudgetChange,
  generations,
  onGenerationsChange,
  populationSize,
  onPopulationSizeChange,
  dateInvalid,
}: OptimizeBasicOptionsProps) {
  return (
    <div className="space-y-4">
      <div className="space-y-1">
        <label htmlFor="optimize-start-date" className="text-sm font-medium block">
          Start Date
        </label>
        <Input
          id="optimize-start-date"
          type="date"
          value={startDate}
          onChange={(e) => onStartDateChange(e.target.value)}
        />
      </div>
      <div className="space-y-1">
        <label htmlFor="optimize-end-date" className="text-sm font-medium block">
          End Date
        </label>
        <Input
          id="optimize-end-date"
          type="date"
          value={endDate}
          onChange={(e) => onEndDateChange(e.target.value)}
        />
      </div>
      {dateInvalid && (
        <p className="text-sm text-destructive">
          End date must be after start date
        </p>
      )}
      <NumericInput
        label="Budget"
        hint="Initial capital for optimization"
        value={budget}
        onChange={(v) => onBudgetChange(v ?? 0)}
        min={1}
      />
      <NumericInput
        label="Generations"
        hint="Number of optimization generations"
        value={generations}
        onChange={(v) => onGenerationsChange(v ?? 1)}
        min={1}
        max={500}
        step={1}
      />
      <NumericInput
        label="Population Size"
        hint="Population per generation"
        value={populationSize}
        onChange={(v) => onPopulationSizeChange(v ?? 2)}
        min={2}
        max={200}
        step={1}
      />
    </div>
  );
}
