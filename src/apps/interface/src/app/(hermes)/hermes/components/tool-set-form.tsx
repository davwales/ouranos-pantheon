"use client";

import {
  MAX_TOOL_ADDITIONAL_DESCRIPTION_LENGTH,
  MAX_TOOL_SET_INSTRUCTIONS_LENGTH,
  TOOL_SOURCE_LABELS,
  toolKey,
} from "@/app/(hermes)/hermes/components/tool-set-constants";
import { ToolToggleCard } from "@/app/(hermes)/hermes/components/tool-toggle-card";
import {
  type ToolSetFormInput,
  type ToolSetToolFormInput,
} from "@/app/(hermes)/hermes/types";
import AutosizeTextarea from "@/components/shared/autosize-textarea";
import { ConfirmationButton } from "@/components/shared/confirmation-button";
import { Typography } from "@/components/shared/typography";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import {
  type AvailableTool,
  type SaveToolSetInput,
  type ToolSet,
  type ToolSource,
} from "@/lib/api/hermes";
import React, { type ChangeEvent, useMemo, useState } from "react";

type ToolRow = {
  source: ToolSource;
  name: string;
  description?: string;
  isAvailable: boolean;
};

export function toToolSetFormInput(toolSet: ToolSet): ToolSetFormInput {
  return {
    name: toolSet.name,
    description: toolSet.description ?? "",
    instructions: toolSet.instructions ?? "",
    isPublic: toolSet.isPublic,
    tools: toolSet.tools.map((t) => ({
      source: t.source,
      name: t.name,
      additionalDescription: t.additionalDescription ?? "",
    })),
  };
}

export function toSaveToolSetInput(input: ToolSetFormInput): SaveToolSetInput {
  const optional = (value: string) => value.trim() || null;

  return {
    name: input.name.trim(),
    description: optional(input.description),
    instructions: optional(input.instructions),
    isPublic: input.isPublic,
    tools: input.tools.map((t) => ({
      source: t.source,
      name: t.name,
      additionalDescription: optional(t.additionalDescription),
    })),
  };
}

function buildRows(
  availableTools: AvailableTool[],
  savedTools: ToolSetToolFormInput[],
): Map<ToolSource, ToolRow[]> {
  const availableKeys = new Set(availableTools.map(toolKey));
  const rows: ToolRow[] = [
    ...availableTools.map((t) => ({ ...t, isAvailable: true })),
    ...savedTools
      .filter((t) => !availableKeys.has(toolKey(t)))
      .map((t) => ({ source: t.source, name: t.name, isAvailable: false })),
  ];

  const grouped = new Map<ToolSource, ToolRow[]>();
  for (const row of rows) {
    grouped.set(row.source, [...(grouped.get(row.source) ?? []), row]);
  }
  return grouped;
}

export function ToolSetForm({
  onSave,
  onDelete,
  initial,
  availableTools,
  loading,
  ...props
}: Omit<React.ComponentProps<"form">, "onSubmit"> & {
  onSave: (input: ToolSetFormInput) => void;
  onDelete?: () => void;
  initial?: ToolSetFormInput;
  availableTools: AvailableTool[];
  loading?: boolean;
}) {
  const [toolSet, setToolSet] = useState<ToolSetFormInput>(
    initial ?? {
      name: "",
      description: "",
      instructions: "",
      tools: [],
      isPublic: true,
    },
  );
  const [drafts, setDrafts] = useState<Record<string, string>>(() =>
    Object.fromEntries(
      (initial?.tools ?? []).map((t) => [toolKey(t), t.additionalDescription]),
    ),
  );

  const rows = useMemo(
    () => buildRows(availableTools, initial?.tools ?? []),
    [availableTools, initial],
  );
  const enabledKeys = new Set(toolSet.tools.map(toolKey));

  const isValid =
    toolSet.name.trim() !== "" &&
    toolSet.instructions.length <= MAX_TOOL_SET_INSTRUCTIONS_LENGTH &&
    toolSet.tools.every(
      (t) =>
        t.additionalDescription.length <= MAX_TOOL_ADDITIONAL_DESCRIPTION_LENGTH,
    );

  const setEnabled = (row: ToolRow, isEnabled: boolean) => {
    setToolSet((prev) => ({
      ...prev,
      tools: isEnabled
        ? [
            ...prev.tools,
            {
              source: row.source,
              name: row.name,
              additionalDescription: drafts[toolKey(row)] ?? "",
            },
          ]
        : prev.tools.filter((t) => toolKey(t) !== toolKey(row)),
    }));
  };

  const setAdditionalDescription = (row: ToolRow, value: string) => {
    setDrafts((prev) => ({ ...prev, [toolKey(row)]: value }));
    setToolSet((prev) => ({
      ...prev,
      tools: prev.tools.map((t) =>
        toolKey(t) === toolKey(row) ? { ...t, additionalDescription: value } : t,
      ),
    }));
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (isValid) {
      onSave(toolSet);
    }
  };

  return (
    <form {...props} onSubmit={handleSubmit}>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Name</Typography>
        <Input
          type="text"
          aria-label="Name"
          value={toolSet.name}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setToolSet((prev) => ({ ...prev, name: e.target.value }))
          }
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Description</Typography>
        <Input
          type="text"
          aria-label="Description"
          value={toolSet.description}
          placeholder="What this tool set is for"
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setToolSet((prev) => ({ ...prev, description: e.target.value }))
          }
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <div>
          <Typography variant="h4">Instructions</Typography>
          <Typography variant="muted">
            Added to the system prompt whenever this set’s tools are offered.
          </Typography>
        </div>
        <div className="space-y-1">
          <AutosizeTextarea
            aria-label="Instructions"
            value={toolSet.instructions}
            maxLength={MAX_TOOL_SET_INSTRUCTIONS_LENGTH}
            placeholder="e.g. Call a tool whenever it helps the reply, without asking the user first."
            onChange={(e: ChangeEvent<HTMLTextAreaElement>) =>
              setToolSet((prev) => ({ ...prev, instructions: e.target.value }))
            }
            className="w-full"
          />
          <Typography variant="muted" className="text-right text-xs">
            {toolSet.instructions.length}/{MAX_TOOL_SET_INSTRUCTIONS_LENGTH}
          </Typography>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4 items-center">
        <Typography variant="h4">Public</Typography>
        <input
          type="checkbox"
          aria-label="Public"
          checked={toolSet.isPublic}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setToolSet((prev) => ({ ...prev, isPublic: e.target.checked }))
          }
          className="h-5 w-5 cursor-pointer"
        />
      </div>

      <Separator className="my-4" />

      <Typography variant="h3">Tools</Typography>
      {rows.size === 0 && (
        <Typography variant="muted">No tools are available.</Typography>
      )}
      {[...rows.entries()].map(([source, sourceRows]) => (
        <div key={source} className="mt-4 space-y-3">
          <Typography variant="h4">{TOOL_SOURCE_LABELS[source]}</Typography>
          {sourceRows.map((row) => (
            <ToolToggleCard
              key={toolKey(row)}
              name={row.name}
              description={row.description}
              isAvailable={row.isAvailable}
              isEnabled={enabledKeys.has(toolKey(row))}
              additionalDescription={drafts[toolKey(row)] ?? ""}
              onEnabledChange={(isEnabled) => setEnabled(row, isEnabled)}
              onAdditionalDescriptionChange={(value) =>
                setAdditionalDescription(row, value)
              }
            />
          ))}
        </div>
      ))}

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
            title="Delete Tool Set"
            description="Are you sure you want to delete this tool set? Models and conversations using it will have no tools."
            onConfirm={onDelete}
            disabled={loading}
            variant="destructive"
            type="button"
            className="w-full md:w-40"
          >
            Delete Tool Set
          </ConfirmationButton>
        )}
      </div>
    </form>
  );
}
