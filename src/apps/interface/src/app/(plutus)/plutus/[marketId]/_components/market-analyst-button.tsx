"use client";

import { AssistantButton } from "@/components/shared/ai-assistant";
import { Typography } from "@/components/shared/typography";
import { type AssistantActionHandlers } from "@/lib/api/assistant";
import { MARKET_ANALYST_ENDPOINT } from "@/lib/api/plutus";
import { type MarketAnalystContext } from "@/lib/api/plutus-types";

const TOOL_LABELS = {
  find_symbols: "matching symbols",
  get_symbol: "a symbol",
  get_signal_rankings: "signal rankings",
  get_forecast_leaders: "forecast leaders",
  get_activity_hours: "trading hours",
  set_explorer_view: "the table view",
};

export function MarketAnalystButton({
  context,
  suggestions,
  actions,
}: {
  context: MarketAnalystContext;
  suggestions: string[];
  actions?: AssistantActionHandlers;
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
      toolLabels={TOOL_LABELS}
      actions={actions}
    />
  );
}
