"use client";

import AutosizeTextarea from "@/components/shared/autosize-textarea";
import { Button } from "@/components/ui/button";
import { ArrowDown, ArrowUp, X } from "lucide-react";

type InstructionStepRowProps = {
  index: number;
  text: string;
  onTextChange: (index: number, value: string) => void;
  onMoveUp: (index: number) => void;
  onMoveDown: (index: number) => void;
  onRemove: (index: number) => void;
  canMoveUp: boolean;
  canMoveDown: boolean;
  canRemove: boolean;
  disabled: boolean;
  error?: string;
};

export function InstructionStepRow({
  index,
  text,
  onTextChange,
  onMoveUp,
  onMoveDown,
  onRemove,
  canMoveUp,
  canMoveDown,
  canRemove,
  disabled,
  error,
}: InstructionStepRowProps) {
  const displayIndex = index + 1;
  const textId = `step-${index}-text`;

  return (
    <div className="flex items-stretch gap-2">
      <div
        className="mt-2 self-start flex size-7 shrink-0 items-center justify-center rounded-full bg-muted text-sm font-medium tabular-nums"
        aria-hidden="true"
      >
        {displayIndex}
      </div>

      <div className="flex min-w-0 flex-1 flex-col gap-1">
        <AutosizeTextarea
          id={textId}
          value={text}
          onChange={(e) => onTextChange(index, e.target.value)}
          placeholder={`Describe step ${displayIndex}…`}
          aria-label={`Text for step ${displayIndex}`}
          aria-invalid={error ? "true" : "false"}
          aria-describedby={error ? `${textId}-error` : undefined}
          disabled={disabled}
          className="min-h-12!"
        />
        {error && (
          <p id={`${textId}-error`} className="text-sm text-destructive" role="alert">
            {error}
          </p>
        )}
      </div>

      <div className="flex gap-1">
        <Button
          type="button"
          variant="outline"
          size="icon-sm"
          className="h-auto!"
          onClick={() => onMoveUp(index)}
          disabled={disabled || !canMoveUp}
          aria-label={`Move step ${displayIndex} up`}
        >
          <ArrowUp />
        </Button>
        <Button
          type="button"
          variant="outline"
          size="icon-sm"
          className="h-auto!"
          onClick={() => onMoveDown(index)}
          disabled={disabled || !canMoveDown}
          aria-label={`Move step ${displayIndex} down`}
        >
          <ArrowDown />
        </Button>
        <Button
          type="button"
          variant="outline"
          size="icon-sm"
          className="h-auto!"
          onClick={() => onRemove(index)}
          disabled={disabled || !canRemove}
          aria-label={`Remove step ${displayIndex}`}
        >
          <X />
        </Button>
      </div>
    </div>
  );
}
