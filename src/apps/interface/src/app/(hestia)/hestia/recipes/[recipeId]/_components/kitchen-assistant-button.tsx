import { AssistantButton } from "@/components/shared/ai-assistant";
import { Typography } from "@/components/shared/typography";
import { type AssistantActionHandlers } from "@/lib/api/assistant";
import { KITCHEN_ASSISTANT_ENDPOINT } from "@/lib/api/hestia";

export function KitchenAssistantButton({
  recipeId,
  actions,
}: {
  recipeId: string;
  actions?: AssistantActionHandlers;
}) {
  return (
    <AssistantButton
      title="Kitchen Assistant"
      description="Ask about substitutions, scaling, timing or technique."
      assistantName="Kitchen Assistant"
      placeholder="Ask about this recipe..."
      endpoint={KITCHEN_ASSISTANT_ENDPOINT}
      context={{ recipeId }}
      actions={actions}
      emptyState={
        <Typography variant="muted">
          Ask anything about this recipe, like “Can I make this dairy-free?”
        </Typography>
      }
    />
  );
}
