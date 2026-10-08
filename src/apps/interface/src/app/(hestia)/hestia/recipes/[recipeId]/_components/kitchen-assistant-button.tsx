"use client";

import { AssistantButton } from "@/components/shared/ai-assistant";
import { Typography } from "@/components/shared/typography";
import { KITCHEN_ASSISTANT_ENDPOINT } from "@/lib/api/hestia";

export function KitchenAssistantButton({ recipeId }: { recipeId: string }) {
  return (
    <AssistantButton
      title="Kitchen Assistant"
      description="Ask about substitutions, scaling, timing or technique."
      assistantName="Kitchen Assistant"
      placeholder="Ask about this recipe..."
      endpoint={KITCHEN_ASSISTANT_ENDPOINT}
      context={{ recipeId }}
      emptyState={
        <Typography variant="muted">
          Ask anything about this recipe, like “Can I make this dairy-free?”
        </Typography>
      }
    />
  );
}
