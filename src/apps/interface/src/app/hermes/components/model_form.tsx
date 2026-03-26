"use client";

import AutosizeTextarea from "@/app/components/autosize-textarea";
import { ConfirmationButton } from "@/app/components/confirmation-button";
import { Typography } from "@/app/components/typography";
import { ModelFormInput } from "@/app/hermes/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import React, { ChangeEvent, useMemo, useState } from "react";

export function ModelForm({
  onSave,
  onDelete,
  submitText,
  initial,
  loading,
  ...props
}: React.ComponentProps<"form"> & {
  onSave?: (input: ModelFormInput) => void;
  submitText?: string;
  onDelete?: () => void;
  initial?: ModelFormInput;
  loading?: boolean;
}) {
  const [model, setModel] = useState<ModelFormInput>(
    initial || {
      name: "",
      modelIdentifier: "",
      systemPrompt: "",
      temperature: null,
      maxTokens: null,
      repeatPenalty: null,
      isDefault: false,
      isPublic: true,
    },
  );

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (onSave) {
      onSave(model);
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
          value={model.name}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setModel((prev) => ({ ...prev, name: e.target.value }))
          }
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Model Identifier</Typography>
        <Input
          type="text"
          readOnly={isReadOnly}
          value={model.modelIdentifier}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setModel((prev) => ({ ...prev, modelIdentifier: e.target.value }))
          }
          placeholder="e.g. llama3.2"
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">System Prompt</Typography>
        <AutosizeTextarea
          value={model.systemPrompt}
          onChange={(e: ChangeEvent<HTMLTextAreaElement>) =>
            setModel((prev) => ({ ...prev, systemPrompt: e.target.value }))
          }
          placeholder="Base system-level instructions..."
          disabled={isReadOnly}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Temperature</Typography>
        <Input
          type="number"
          readOnly={isReadOnly}
          value={model.temperature?.toFixed(2) ?? 0.7}
          step={0.01}
          onChange={(e: ChangeEvent<HTMLInputElement>) => {
            const temperature = parseFloat(e.target.value);
            if (!isNaN(temperature)) {
              setModel((prev) => ({ ...prev, temperature }));
            }
          }}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Max Tokens</Typography>
        <Input
          type="number"
          readOnly={isReadOnly}
          value={model.maxTokens ?? 128}
          step={1}
          onChange={(e: ChangeEvent<HTMLInputElement>) => {
            const maxTokens = parseInt(e.target.value);
            if (!isNaN(maxTokens)) {
              setModel((prev) => ({ ...prev, maxTokens }));
            }
          }}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Repeat Penalty</Typography>
        <Input
          type="number"
          readOnly={isReadOnly}
          value={model.repeatPenalty?.toFixed(2) ?? 1.0}
          step={0.01}
          onChange={(e: ChangeEvent<HTMLInputElement>) => {
            const repeatPenalty = parseFloat(e.target.value);
            if (!isNaN(repeatPenalty)) {
              setModel((prev) => ({ ...prev, repeatPenalty }));
            }
          }}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4 items-center">
        <Typography variant="h4">Default</Typography>
        <input
          type="checkbox"
          checked={model.isDefault}
          disabled={isReadOnly}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setModel((prev) => ({ ...prev, isDefault: e.target.checked }))
          }
          className="h-5 w-5 cursor-pointer"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4 items-center">
        <Typography variant="h4">Public</Typography>
        <input
          type="checkbox"
          checked={model.isPublic}
          disabled={isReadOnly}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setModel((prev) => ({ ...prev, isPublic: e.target.checked }))
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
                  title="Delete Model"
                  description="Are you sure you want to delete this model? This action cannot be undone."
                  onConfirm={onDelete}
                  disabled={loading}
                  variant="destructive"
                  type="button"
                  className="w-full md:w-40"
                >
                  Delete Model
                </ConfirmationButton>
              )}
            </div>
          )}
        </div>
      )}
    </form>
  );
}
