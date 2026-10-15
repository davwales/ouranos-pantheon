"use client";

import { type AssistantChatAction } from "@/components/shared/ai-assistant/use-assistant-chat";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Check, Loader2 } from "lucide-react";

export function ActionCard({
  description,
  action,
  onApply,
}: {
  description: string;
  action: AssistantChatAction;
  onApply: () => void;
}) {
  return (
    <div className="my-2 flex flex-col gap-2 rounded-lg border bg-muted/30 p-3 text-sm">
      <span>{description}</span>
      {action.status === "failed" && action.error && (
        <span role="alert" className="text-destructive">
          {action.error}
        </span>
      )}
      <div className="flex justify-end">
        {action.status === "applied" ? (
          <Badge variant="secondary">
            <Check />
            Applied
          </Badge>
        ) : (
          <Button
            size="sm"
            variant="outline"
            disabled={action.status === "applying"}
            onClick={onApply}
          >
            {action.status === "applying" && <Loader2 className="animate-spin" />}
            {action.status === "failed" ? "Retry" : "Apply"}
          </Button>
        )}
      </div>
    </div>
  );
}
