"use client";

import { Input } from "@/components/ui/input";

export type NumericInputProps = {
  label?: string;
  hint?: string;
  value: number | null | undefined;
  onChange: (v: number | null) => void;
  min?: number;
  max?: number;
  step?: number;
  id?: string;
  className?: string;
};

function formatWithCommas(value: number): string {
  return value.toLocaleString("en-US", {
    maximumFractionDigits: 10,
  });
}

function stripNonNumeric(value: string): string {
  return value.replace(/[^0-9.\-]/g, "");
}

export function NumericInput({
  label,
  hint,
  value,
  onChange,
  min,
  max,
  step,
  id,
  className,
}: NumericInputProps) {
  const displayValue = value != null ? formatWithCommas(value) : "";

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const raw = stripNonNumeric(e.target.value);

    if (raw === "" || raw === "-") {
      onChange(null);
      return;
    }

    const parsed = parseFloat(raw);
    onChange(isNaN(parsed) ? null : parsed);
  };

  const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    const raw = stripNonNumeric(e.target.value);

    if (raw === "" || raw === "-") {
      onChange(null);
      return;
    }

    let parsed = parseFloat(raw);
    if (isNaN(parsed)) {
      onChange(null);
      return;
    }

    if (min != null && parsed < min) {
      parsed = min;
    }

    if (max != null && parsed > max) {
      parsed = max;
    }

    onChange(parsed);
  };

  const inputElement = (
    <Input
      id={id}
      type="text"
      inputMode={
        Number.isInteger(step) && step != null && min != null && min >= 0
          ? "numeric"
          : "decimal"
      }
      value={displayValue}
      onChange={handleChange}
      onBlur={handleBlur}
      min={min}
      max={max}
      step={step}
      className={className}
    />
  );

  if (!label) {
    return inputElement;
  }

  return (
    <div className="space-y-1">
      <label htmlFor={id} className="text-sm font-medium block">
        {label}
        {hint && (
          <span className="text-muted-foreground text-xs ml-1">({hint})</span>
        )}
      </label>
      {inputElement}
    </div>
  );
}
