import { abbreviateNumber } from "@/components/shared/pretty-number/abbreviate-number";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { type StrategyRecommendation } from "@/lib/api/plutus";
import { Info } from "lucide-react";
import { POSITION_LIMIT_LABELS } from "./recommendation-constants";

export function RecommendationDetailsPopover({
  recommendation,
}: {
  recommendation: StrategyRecommendation;
}) {
  return (
    <Popover>
      <PopoverTrigger
        className="text-muted-foreground hover:text-foreground shrink-0"
        aria-label={`Details for ${recommendation.symbolName}`}
      >
        <Info className="w-3.5 h-3.5" />
      </PopoverTrigger>
      <PopoverContent className="space-y-2">
        <dl className="grid grid-cols-[auto_1fr] gap-x-3 gap-y-1 text-xs">
          <dt className="text-muted-foreground">Sized by</dt>
          <dd>{POSITION_LIMIT_LABELS[recommendation.limitedBy]}</dd>
          <dt className="text-muted-foreground">Typical volume</dt>
          <dd>{abbreviateNumber(recommendation.typicalDailyVolume)} / day</dd>
        </dl>
        {recommendation.rationale && (
          <p className="text-xs text-muted-foreground">
            {recommendation.rationale}
          </p>
        )}
      </PopoverContent>
    </Popover>
  );
}
