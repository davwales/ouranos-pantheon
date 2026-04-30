"use client";

import { Input } from "@/components/ui/input";

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

export function NumberInput({
  label,
  hint,
  value,
  onChange,
  min,
  max,
  step,
}: {
  label: string;
  hint?: string;
  value: number | null | undefined;
  onChange: (v: number | null) => void;
  min?: number;
  max?: number;
  step?: number;
}) {
  return (
    <div className="space-y-1">
      <FieldLabel hint={hint}>{label}</FieldLabel>
      <Input
        type="number"
        value={value ?? ""}
        onChange={(e) => {
          const v = e.target.value === "" ? null : parseFloat(e.target.value);
          onChange(isNaN(v!) ? null : v);
        }}
        min={min}
        max={max}
        step={step}
      />
    </div>
  );
}
