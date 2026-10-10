"use client";

import { AssistantButton } from "@/components/shared/ai-assistant";
import { Typography } from "@/components/shared/typography";
import { MARKET_ANALYST_ENDPOINT } from "@/lib/api/plutus";
import { type MarketAnalystContext } from "@/lib/api/plutus-types";

export function MarketAnalystButton({
  context,
  suggestions,
}: {
  context: MarketAnalystContext;
  suggestions: string[];
}) {
  return (
    <AssistantButton
      title="Market Analyst"
      description="Ask about the market activity and the symbols you are viewing."
      assistantName="Market Analyst"
      placeholder="Ask about this market..."
      endpoint={MARKET_ANALYST_ENDPOINT}
      context={context}
      emptyState={
        <Typography variant="muted">
          Ask anything about what you see on this page, or start with a suggestion.
        </Typography>
      }
      suggestions={suggestions}
    />
  );
}
