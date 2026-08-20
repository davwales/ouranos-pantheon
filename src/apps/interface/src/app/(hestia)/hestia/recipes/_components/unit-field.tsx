"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectSeparator,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { ChevronLeft } from "lucide-react";
import { useState } from "react";
import { isCustomUnit, OTHER_UNIT_VALUE, UNIT_OPTIONS } from "./unit-options";

type Mode = "select" | "custom";

type UnitFieldProps = {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  ariaLabel: string;
  ariaInvalid?: "true" | "false";
  ariaDescribedBy?: string;
  className?: string;
};

function resolveMode(value: string, override: Mode | null): Mode {
  if (value !== "") {
    return isCustomUnit(value) ? "custom" : "select";
  }
  return override ?? "select";
}

export function UnitField({
  value,
  onChange,
  disabled = false,
  ariaLabel,
  ariaInvalid,
  ariaDescribedBy,
  className,
}: UnitFieldProps) {
  const [modeOverride, setModeOverride] = useState<Mode | null>(() =>
    value === "" ? null : isCustomUnit(value) ? "custom" : "select",
  );

  const mode = resolveMode(value, modeOverride);

  if (mode === "custom") {
    return (
      <div className="flex items-center gap-1">
        <Input
          type="text"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder="Custom unit"
          aria-label={ariaLabel}
          aria-invalid={ariaInvalid}
          aria-describedby={ariaDescribedBy}
          disabled={disabled}
          className="w-32"
        />
        <Button
          type="button"
          variant="outline"
          size="icon-sm"
          onClick={() => {
            setModeOverride("select");
            onChange("");
          }}
          disabled={disabled}
          aria-label={`${ariaLabel} - use list`}
        >
          <ChevronLeft className="size-4" />
        </Button>
      </div>
    );
  }

  return (
    <Select
      value={value}
      onValueChange={(v) => {
        if (v === OTHER_UNIT_VALUE) {
          setModeOverride("custom");
          onChange("");
        } else {
          setModeOverride("select");
          onChange(v);
        }
      }}
    >
      <SelectTrigger
        aria-label={ariaLabel}
        aria-invalid={ariaInvalid}
        aria-describedby={ariaDescribedBy}
        disabled={disabled}
        className={cn("w-32", className)}
      >
        <SelectValue placeholder="Unit" />
      </SelectTrigger>
      <SelectContent>
        {UNIT_OPTIONS.map((unit) => (
          <SelectItem key={unit} value={unit}>
            {unit}
          </SelectItem>
        ))}
        <SelectSeparator />
        <SelectItem value={OTHER_UNIT_VALUE}>Other…</SelectItem>
      </SelectContent>
    </Select>
  );
}
