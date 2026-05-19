"use client";

import { useState, useCallback } from "react";
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

function formatRawWithCommas(raw: string): string {
  if (raw === "" || raw === "-" || raw === "." || raw === "-.") return raw;
  const hasMinus = raw.startsWith("-");
  const core = hasMinus ? raw.slice(1) : raw;
  const dotIdx = core.indexOf(".");
  const intPart = dotIdx === -1 ? core : core.slice(0, dotIdx);
  const fracPart = dotIdx === -1 ? "" : core.slice(dotIdx);
  if (intPart === "" || isNaN(parseInt(intPart, 10))) return raw;
  const withCommas = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
  return (hasMinus ? "-" : "") + withCommas + fracPart;
}

function stripNonNumeric(value: string): string {
  return value.replace(/[^0-9.\-]/g, "");
}

function isValidNumericPattern(value: string): boolean {
  if (value === "" || value === "-" || value === "-." || value === ".") return true;
  const hasMinus = value.startsWith("-");
  const core = hasMinus ? value.slice(1) : value;
  const parts = core.split(".");
  if (parts.length > 2) return false;
  return parts.every((p) => !isNaN(parseFloat(p)) || p === "");
}

function clamp(value: number, min?: number, max?: number): number {
  let result = value;
  if (min != null && result < min) {
    result = min;
  }
  if (max != null && result > max) {
    result = max;
  }
  return result;
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
  const [isFocused, setIsFocused] = useState(false);
  const [rawValue, setRawValue] = useState<string>("");

  const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const noCommas = e.target.value.replace(/,/g, "");
    const stripped = stripNonNumeric(noCommas);
    if (!isValidNumericPattern(stripped)) {
      return;
    }
    setRawValue(stripped);
    if (stripped === "" || stripped === "-" || stripped.endsWith(".") || stripped === "-.") {
      return;
    }
    const parsed = parseFloat(stripped);
    onChange(isNaN(parsed) ? null : parsed);
  }, [onChange]);

  const handleFocus = useCallback(() => {
    setIsFocused(true);
    setRawValue(value != null ? String(value) : "");
  }, [value]);

  const handleBlur = useCallback(
    (e: React.FocusEvent<HTMLInputElement>) => {
      setIsFocused(false);
      const raw = stripNonNumeric(e.target.value.replace(/,/g, ""));
      if (raw === "" || raw === "-" || raw === "-.") {
        setRawValue("");
        onChange(null);
        return;
      }
      let parsed = parseFloat(raw);
      if (isNaN(parsed)) {
        setRawValue("");
        onChange(null);
        return;
      }
      parsed = clamp(parsed, min, max);
      setRawValue(formatWithCommas(parsed));
      onChange(parsed);
    },
    [onChange, min, max],
  );

  const displayValue = isFocused
    ? formatRawWithCommas(rawValue)
    : value != null
      ? formatWithCommas(value)
      : "";

  return (
    <div className="space-y-1">
      {label && (
        <label htmlFor={id} className="text-sm font-medium block">
          {label}
          {hint && (
            <span className="text-muted-foreground text-xs ml-1">({hint})</span>
          )}
        </label>
      )}
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
        onFocus={handleFocus}
        onBlur={handleBlur}
        min={min}
        max={max}
        step={step}
        className={className}
      />
    </div>
  );
}
