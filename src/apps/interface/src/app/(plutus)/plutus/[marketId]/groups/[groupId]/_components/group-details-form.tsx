"use client";

import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

type GroupDetailsFormProps = {
  name: string;
  description: string;
  onNameChange: (name: string) => void;
  onDescriptionChange: (description: string) => void;
};

export function GroupDetailsForm({
  name,
  description,
  onNameChange,
  onDescriptionChange,
}: GroupDetailsFormProps) {
  return (
    <div className="mt-4 space-y-4 max-w-lg">
      <div className="space-y-1">
        <label htmlFor="name" className="text-sm font-medium">
          Name
        </label>
        <Input
          id="name"
          value={name}
          onChange={(e) => onNameChange(e.target.value)}
        />
      </div>
      <div className="space-y-1">
        <label htmlFor="description" className="text-sm font-medium">
          Description (optional)
        </label>
        <Textarea
          id="description"
          value={description}
          onChange={(e) => onDescriptionChange(e.target.value)}
          rows={3}
        />
      </div>
    </div>
  );
}
