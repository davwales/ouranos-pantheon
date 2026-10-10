"use client";

import { type TimeFrameKey } from "@/app/(plutus)/plutus/constants/time-frames";
import { AssistantButton } from "@/components/shared/ai-assistant";
import { Typography } from "@/components/shared/typography";
import { SYMBOL_ANALYST_ENDPOINT } from "@/lib/api/plutus";

const SUGGESTIONS = [
  "Is this a good buy right now?",
  "What are good buy and sell prices after tax?",
  "Explain the current signals.",
  "How reliable is the forecast?",
  "Should I hold or sell my open positions?",
];

export function SymbolAnalystButton({
  symbolId,
  timeFrame,
}: {
  symbolId: string;
  timeFrame: TimeFrameKey;
}) {
  return (
    <AssistantButton
      title="Symbol Analyst"
      description="Ask about price action, signals, forecasts or your positions."
      assistantName="Symbol Analyst"
      placeholder="Ask about this symbol..."
      endpoint={SYMBOL_ANALYST_ENDPOINT}
      context={{ symbolId, timeFrame }}
      emptyState={
        <Typography variant="muted">
          Ask anything about this symbol, or start with a suggestion.
        </Typography>
      }
      suggestions={SUGGESTIONS}
    />
  );
}
