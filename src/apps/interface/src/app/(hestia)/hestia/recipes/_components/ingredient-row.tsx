"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { X } from "lucide-react";
import React from "react";
import { UnitField } from "./unit-field";

type IngredientRowProps = {
  index: number;
  quantity: string;
  unit: string;
  name: string;
  onQuantityChange: (index: number, value: string) => void;
  onUnitChange: (index: number, value: string) => void;
  onNameChange: (index: number, value: string) => void;
  onRemove: (index: number) => void;
  canRemove: boolean;
  disabled: boolean;
  errors?: { quantity?: string; unit?: string; name?: string };
};

export function IngredientRow({
  index,
  quantity,
  unit,
  name,
  onQuantityChange,
  onUnitChange,
  onNameChange,
  onRemove,
  canRemove,
  disabled,
  errors,
}: IngredientRowProps) {
  const displayIndex = index + 1;
  const quantityId = `ingredient-${index}-quantity`;
  const unitId = `ingredient-${index}-unit`;
  const nameId = `ingredient-${index}-name`;

  return (
    <div className="flex flex-wrap items-start gap-2">
      <div className="flex flex-col gap-1">
        <Input
          id={quantityId}
          type="number"
          inputMode="decimal"
          step="any"
          min="0"
          value={quantity}
          onChange={(e) => onQuantityChange(index, e.target.value)}
          placeholder="Qty"
          aria-label={`Quantity for ingredient ${displayIndex}`}
          aria-invalid={errors?.quantity ? "true" : "false"}
          aria-describedby={errors?.quantity ? `${quantityId}-error` : undefined}
          disabled={disabled}
          className="w-24"
        />
        {errors?.quantity && (
          <p
            id={`${quantityId}-error`}
            className="text-sm text-destructive"
            role="alert"
          >
            {errors.quantity}
          </p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <UnitField
          value={unit}
          onChange={(v) => onUnitChange(index, v)}
          disabled={disabled}
          ariaLabel={`Unit for ingredient ${displayIndex}`}
          ariaInvalid={errors?.unit ? "true" : "false"}
          ariaDescribedBy={errors?.unit ? `${unitId}-error` : undefined}
          className="w-32"
        />
        {errors?.unit && (
          <p
            id={`${unitId}-error`}
            className="text-sm text-destructive"
            role="alert"
          >
            {errors.unit}
          </p>
        )}
      </div>

      <div className="flex min-w-48 flex-1 flex-col gap-1">
        <Input
          id={nameId}
          type="text"
          value={name}
          onChange={(e) => onNameChange(index, e.target.value)}
          placeholder="Name"
          aria-label={`Name for ingredient ${displayIndex}`}
          aria-invalid={errors?.name ? "true" : "false"}
          aria-describedby={errors?.name ? `${nameId}-error` : undefined}
          disabled={disabled}
        />
        {errors?.name && (
          <p
            id={`${nameId}-error`}
            className="text-sm text-destructive"
            role="alert"
          >
            {errors.name}
          </p>
        )}
      </div>

      <Button
        type="button"
        variant="outline"
        size="icon-sm"
        onClick={() => onRemove(index)}
        disabled={disabled || !canRemove}
        aria-label={`Remove ingredient ${displayIndex}`}
      >
        <X />
      </Button>
    </div>
  );
}
