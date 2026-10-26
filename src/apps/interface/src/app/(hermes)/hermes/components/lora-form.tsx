"use client";

import { ConfirmationButton } from "@/components/shared/confirmation-button";
import { Typography } from "@/components/shared/typography";
import { type LoraFormInput } from "@/app/(hermes)/hermes/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import React, { ChangeEvent, useState } from "react";
import { useUnsavedChangesGuard } from "@/hooks/use-unsaved-changes-guard";
import { hasChanges } from "@/lib/has-changes";

export const MIN_LORA_WEIGHT = -2;
export const MAX_LORA_WEIGHT = 2;

export function LoraForm({
  onSave,
  onDelete,
  initial,
  loading,
  ...props
}: React.ComponentProps<"form"> & {
  onSave: (input: LoraFormInput) => void;
  onDelete?: () => void;
  initial?: LoraFormInput;
  loading?: boolean;
}) {
  const [baseline] = useState<LoraFormInput>(
    () =>
      initial ?? { name: "", identifier: "", weight: 1, isPublic: true },
  );
  const [lora, setLora] = useState<LoraFormInput>(baseline);
  useUnsavedChangesGuard(hasChanges(lora, baseline));

  const isValid =
    lora.name.trim() !== "" &&
    lora.identifier.trim() !== "" &&
    lora.weight >= MIN_LORA_WEIGHT &&
    lora.weight <= MAX_LORA_WEIGHT;

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (isValid) {
      onSave(lora);
    }
  };

  return (
    <form {...props} onSubmit={handleSubmit}>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Name</Typography>
        <Input
          type="text"
          value={lora.name}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setLora((prev) => ({ ...prev, name: e.target.value }))
          }
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <div>
          <Typography variant="h4">Identifier</Typography>
          <Typography variant="muted">
            The LoRA name as SwarmUI knows it, without the file extension.
          </Typography>
        </div>
        <Input
          type="text"
          value={lora.identifier}
          placeholder="e.g. add-detail-xl"
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setLora((prev) => ({ ...prev, identifier: e.target.value }))
          }
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <div>
          <Typography variant="h4">Weight</Typography>
          <Typography variant="muted">
            Strength from {MIN_LORA_WEIGHT} to {MAX_LORA_WEIGHT}; 1 is the
            LoRA’s usual strength.
          </Typography>
        </div>
        <Input
          type="number"
          value={lora.weight}
          min={MIN_LORA_WEIGHT}
          max={MAX_LORA_WEIGHT}
          step={0.05}
          onChange={(e: ChangeEvent<HTMLInputElement>) => {
            const weight = parseFloat(e.target.value);
            if (!isNaN(weight)) {
              setLora((prev) => ({ ...prev, weight }));
            }
          }}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4 items-center">
        <Typography variant="h4">Public</Typography>
        <input
          type="checkbox"
          checked={lora.isPublic}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setLora((prev) => ({ ...prev, isPublic: e.target.checked }))
          }
          className="h-5 w-5 cursor-pointer"
        />
      </div>

      <Separator className="my-4" />

      <div className="grid grid-cols-1 gap-4 md:flex md:justify-between">
        <Button
          type="submit"
          disabled={loading || !isValid}
          className="w-full md:w-40"
        >
          Save
        </Button>
        {onDelete && (
          <ConfirmationButton
            title="Delete LoRA"
            description="Are you sure you want to delete this LoRA? Conversations using it will stop applying it."
            onConfirm={onDelete}
            disabled={loading}
            variant="destructive"
            type="button"
            className="w-full md:w-40"
          >
            Delete LoRA
          </ConfirmationButton>
        )}
      </div>
    </form>
  );
}
