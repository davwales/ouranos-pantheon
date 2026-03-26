"use client";

import AutosizeTextarea from "@/app/components/autosize-textarea";
import { ConfirmationButton } from "@/app/components/confirmation-button";
import { Typography } from "@/app/components/typography";
import { PersonaFormInput } from "@/app/hermes/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import React, { ChangeEvent, useMemo, useState } from "react";

export function PersonaForm({
  onSave,
  onDelete,
  submitText,
  initial,
  loading,
  ...props
}: React.ComponentProps<"form"> & {
  onSave?: (input: PersonaFormInput) => void;
  submitText?: string;
  onDelete?: () => void;
  initial?: PersonaFormInput;
  loading?: boolean;
}) {
  const [persona, setPersona] = useState<PersonaFormInput>(
    initial || {
      name: "",
      description: "",
      personality: null,
      scenario: null,
      isDefault: false,
      isPublic: true,
    },
  );

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (onSave) {
      onSave(persona);
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
          value={persona.name}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setPersona((prev) => ({ ...prev, name: e.target.value }))
          }
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Description</Typography>
        <AutosizeTextarea
          value={persona.description}
          onChange={(e: ChangeEvent<HTMLTextAreaElement>) =>
            setPersona((prev) => ({ ...prev, description: e.target.value }))
          }
          placeholder="Who is this persona? Describe their background and identity..."
          disabled={isReadOnly}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Personality</Typography>
        <AutosizeTextarea
          value={persona.personality ?? ""}
          onChange={(e: ChangeEvent<HTMLTextAreaElement>) =>
            setPersona((prev) => ({
              ...prev,
              personality: e.target.value || null,
            }))
          }
          placeholder="Tone and behavioral traits (e.g. warm, concise, slightly sarcastic)..."
          disabled={isReadOnly}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Scenario</Typography>
        <AutosizeTextarea
          value={persona.scenario ?? ""}
          onChange={(e: ChangeEvent<HTMLTextAreaElement>) =>
            setPersona((prev) => ({
              ...prev,
              scenario: e.target.value || null,
            }))
          }
          placeholder="Situational context (e.g. You are helping debug code, You are running a medieval tavern)..."
          disabled={isReadOnly}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4 items-center">
        <Typography variant="h4">Default</Typography>
        <input
          type="checkbox"
          checked={persona.isDefault}
          disabled={isReadOnly}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setPersona((prev) => ({ ...prev, isDefault: e.target.checked }))
          }
          className="h-5 w-5 cursor-pointer"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4 items-center">
        <Typography variant="h4">Public</Typography>
        <input
          type="checkbox"
          checked={persona.isPublic}
          disabled={isReadOnly}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setPersona((prev) => ({ ...prev, isPublic: e.target.checked }))
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
                  title="Delete Persona"
                  description="Are you sure you want to delete this persona? This action cannot be undone."
                  onConfirm={onDelete}
                  disabled={loading}
                  variant="destructive"
                  type="button"
                  className="w-full md:w-40"
                >
                  Delete Persona
                </ConfirmationButton>
              )}
            </div>
          )}
        </div>
      )}
    </form>
  );
}
