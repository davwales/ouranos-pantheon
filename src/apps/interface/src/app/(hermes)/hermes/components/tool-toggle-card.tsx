"use client";

import { MAX_TOOL_ADDITIONAL_DESCRIPTION_LENGTH } from "@/app/(hermes)/hermes/components/tool-set-constants";
import AutosizeTextarea from "@/components/shared/autosize-textarea";
import { Typography } from "@/components/shared/typography";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { type ChangeEvent } from "react";

export function ToolToggleCard({
  name,
  description,
  isAvailable,
  isEnabled,
  additionalDescription,
  onEnabledChange,
  onAdditionalDescriptionChange,
}: {
  name: string;
  description?: string;
  isAvailable: boolean;
  isEnabled: boolean;
  additionalDescription: string;
  onEnabledChange: (isEnabled: boolean) => void;
  onAdditionalDescriptionChange: (additionalDescription: string) => void;
}) {
  const inputId = `tool-${name}`;

  return (
    <Card className="py-4">
      <CardContent className="space-y-3">
        <div className="flex items-center justify-between gap-4">
          <label
            htmlFor={inputId}
            className="flex items-center gap-2 font-mono text-sm font-medium cursor-pointer"
          >
            {name}
            {!isAvailable && <Badge variant="destructive">Unavailable</Badge>}
          </label>
          <input
            id={inputId}
            aria-label={name}
            type="checkbox"
            checked={isEnabled}
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              onEnabledChange(e.target.checked)
            }
            className="h-5 w-5 cursor-pointer"
          />
        </div>

        {description && (
          <Typography variant="muted" className="whitespace-pre-line">
            {description}
          </Typography>
        )}
        {!isAvailable && (
          <Typography variant="muted">
            This tool no longer exists. Disable it to remove it from the set.
          </Typography>
        )}

        {isEnabled && (
          <div className="space-y-1">
            <AutosizeTextarea
              aria-label={`Additional description for ${name}`}
              value={additionalDescription}
              maxLength={MAX_TOOL_ADDITIONAL_DESCRIPTION_LENGTH}
              placeholder="Extra guidance appended to the tool description, e.g. only use it when the user asks for an image."
              onChange={(e: ChangeEvent<HTMLTextAreaElement>) =>
                onAdditionalDescriptionChange(e.target.value)
              }
              className="w-full"
            />
            <Typography variant="muted" className="text-right text-xs">
              {additionalDescription.length}/
              {MAX_TOOL_ADDITIONAL_DESCRIPTION_LENGTH}
            </Typography>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
