"use client";

import AutosizeTextarea from "@/app/components/autosize-textarea";
import { ConfirmationButton } from "@/app/components/confirmation-button";
import { Typography } from "@/app/components/typography";
import { AssistantFormInput } from "@/app/hermes/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import React, { ChangeEvent, useMemo, useState } from "react";

export function AssistantForm({
  onSave,
  onDelete,
  submitText,
  initial,
  loading,
  ...props
}: React.ComponentProps<"form"> & {
  onSave?: (input: AssistantFormInput) => void;
  submitText?: string;
  onDelete?: () => void;
  initial?: AssistantFormInput;
  loading?: boolean;
}) {
  const [assistant, setAssistant] = useState<AssistantFormInput>(
    initial || {
      model: "",
      systemPrompt: "",
      userName: "User",
      assistantName: "Assistant",
    }
  );

  const handleModelChange = (event: ChangeEvent<HTMLInputElement>) => {
    setAssistant((prev) => ({
      ...prev,
      model: event.target.value,
    }));
  };

  const handleSystemPromptChange = (
    event: ChangeEvent<HTMLTextAreaElement>
  ) => {
    setAssistant((prev) => ({
      ...prev,
      systemPrompt: event.target.value,
    }));
  };

  const handleAssistantNameChange = (event: ChangeEvent<HTMLInputElement>) => {
    setAssistant((prev) => ({
      ...prev,
      assistantName: event.target.value,
    }));
  };

  const handleUserNameChange = (event: ChangeEvent<HTMLInputElement>) => {
    setAssistant((prev) => ({
      ...prev,
      userName: event.target.value,
    }));
  };

  const handleTemperatureChange = (event: ChangeEvent<HTMLInputElement>) => {
    const temperature = parseFloat(event.target.value);
    if (temperature) {
      setAssistant((prev) => ({
        ...prev,
        temperature,
      }));
    }
  };

  const handleMaxTokensChange = (event: ChangeEvent<HTMLInputElement>) => {
    const maxTokens = parseInt(event.target.value);
    if (maxTokens) {
      setAssistant((prev) => ({
        ...prev,
        maxTokens,
      }));
    }
  };

  const handleRepeatPenaltyChange = (event: ChangeEvent<HTMLInputElement>) => {
    const repeatPenalty = parseFloat(event.target.value);
    if (repeatPenalty) {
      setAssistant((prev) => ({
        ...prev,
        repeatPenalty,
      }));
    }
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();

    if (onSave) {
      onSave(assistant);
    }
  };

  const isReadOnly = useMemo(() => !Boolean(onSave), [onSave]);

  return (
    <form {...props} onSubmit={handleSubmit}>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Model</Typography>
        <Input
          type="text"
          readOnly={isReadOnly}
          value={assistant.model}
          onChange={handleModelChange}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">System Prompt</Typography>
        <AutosizeTextarea
          value={assistant.systemPrompt}
          onChange={handleSystemPromptChange}
          placeholder="Type your system prompt..."
          disabled={isReadOnly}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Assistant Name</Typography>
        <Input
          type="text"
          readOnly={isReadOnly}
          value={assistant.assistantName}
          onChange={handleAssistantNameChange}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">User Name</Typography>
        <Input
          type="text"
          readOnly={isReadOnly}
          value={assistant.userName}
          onChange={handleUserNameChange}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Temperature</Typography>
        <Input
          type="number"
          readOnly={isReadOnly}
          value={assistant.temperature?.toFixed(2) || 0.7}
          step={0.01}
          onChange={handleTemperatureChange}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Max Tokens</Typography>
        <Input
          type="number"
          readOnly={isReadOnly}
          value={assistant.maxTokens || 128}
          step={1}
          onChange={handleMaxTokensChange}
          className="w-full"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <Typography variant="h4">Repeat Penalty</Typography>
        <Input
          type="number"
          readOnly={isReadOnly}
          value={assistant.repeatPenalty?.toFixed(2) || 1.0}
          step={0.01}
          onChange={handleRepeatPenaltyChange}
          className="w-full"
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
                  title="Delete Assistant"
                  description="Are you sure you want to delete this assistant? This action cannot be undone."
                  onConfirm={onDelete}
                  disabled={loading}
                  variant="destructive"
                  type="button"
                  className="w-full md:w-40"
                >
                  Delete Assistant
                </ConfirmationButton>
              )}
            </div>
          )}
        </div>
      )}
    </form>
  );
}
