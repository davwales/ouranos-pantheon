"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Plus } from "lucide-react";
import { useState } from "react";

export type AddManualItemFormProps = {
  onAdd: (text: string) => Promise<boolean>;
  adding: boolean;
};

export function AddManualItemForm({ onAdd, adding }: AddManualItemFormProps) {
  const [text, setText] = useState("");
  const trimmed = text.trim();
  const canSubmit = trimmed !== "" && !adding;

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!canSubmit) {
      return;
    }
    const succeeded = await onAdd(trimmed);
    if (succeeded) {
      setText("");
    }
  };

  return (
    <form onSubmit={handleSubmit} className="flex items-center gap-2">
      <Input
        type="text"
        value={text}
        onChange={(e) => setText(e.target.value)}
        placeholder="Add an item..."
        disabled={adding}
        className="flex-1"
      />
      <Button type="submit" disabled={!canSubmit}>
        <Plus className="size-4" />
        Add
      </Button>
    </form>
  );
}
