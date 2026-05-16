"use client";

import AutosizeTextarea from "@/components/shared/autosize-textarea";
import { ConfirmationButton } from "@/components/shared/confirmation-button";
import { Typography } from "@/components/shared/typography";
import { TraitFormInput } from "@/app/(hermes)/hermes/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import React, { ChangeEvent, useMemo, useState } from "react";

export function TraitForm({
  onSave,
  onDelete,
  submitText,
  initial,
  loading,
  ...props
}: React.ComponentProps<"form"> & {
  onSave?: (input: TraitFormInput) => void;
  submitText?: string;
  onDelete?: () => void;
  initial?: TraitFormInput;
  loading?: boolean;
}) {
  const [trait, setTrait] = useState<TraitFormInput>(
    initial || {
      name: "",
      content: "",
      isPublic: true,
    },
  );

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (onSave) {
      onSave(trait);
    }
  };

  const isReadOnly = useMemo(() => !Boolean(onSave), [onSave]);

  return (
    <form {...props} onSubmit={handleSubmit}>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Name</Typography>
        <Input
          type="text"
          readOnly={isReadOnly}
          value={trait.name}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setTrait((prev) => ({ ...prev, name: e.target.value }))
          }
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Content</Typography>
        <AutosizeTextarea
          value={trait.content}
          onChange={(e: ChangeEvent<HTMLTextAreaElement>) =>
            setTrait((prev) => ({ ...prev, content: e.target.value }))
          }
          placeholder="The context or instruction to inject into the conversation..."
          disabled={isReadOnly}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4 items-center">
        <Typography variant="h4">Public</Typography>
        <input
          type="checkbox"
          checked={trait.isPublic}
          disabled={isReadOnly}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setTrait((prev) => ({ ...prev, isPublic: e.target.checked }))
          }
          className="h-5 w-5 cursor-pointer"
        />
      </div>

      {!isReadOnly && (
        <div>
          <Separator className="my-4" />

          {(onSave || onDelete) && (
            <div className="grid grid-cols-1 gap-4 md:flex md:justify-between">
              {onSave && (
                <Button type="submit" className="w-full md:w-40">
                  {submitText ?? "Save"}
                </Button>
              )}
              {onDelete && (
                <ConfirmationButton
                  title="Delete Trait"
                  description="Are you sure you want to delete this trait? This action cannot be undone."
                  onConfirm={onDelete}
                  disabled={loading}
                  variant="destructive"
                  type="button"
                  className="w-full md:w-40"
                >
                  Delete Trait
                </ConfirmationButton>
              )}
            </div>
          )}
        </div>
      )}
    </form>
  );
}
