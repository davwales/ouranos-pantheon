import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { type StrategyMode } from "@/lib/api/plutus";

const MODE_DESCRIPTIONS: Record<StrategyMode, string> = {
  Swing: "Buy the symbols your signals rank highest and hold them for days.",
  Flip: "Buy at the recent sell-side price and sell at the recent buy-side price, re-quoting at every check-in.",
};

const MODES: StrategyMode[] = ["Swing", "Flip"];

export function StrategyModeCard({
  mode,
  onChange,
}: {
  mode: StrategyMode;
  onChange: (mode: StrategyMode) => void;
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Mode</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        <div className="grid grid-cols-2 gap-2">
          {MODES.map((m) => (
            <Button
              key={m}
              type="button"
              variant={m === mode ? "default" : "outline"}
              size="sm"
              onClick={() => onChange(m)}
            >
              {m}
            </Button>
          ))}
        </div>
        <p className="text-xs text-muted-foreground">{MODE_DESCRIPTIONS[mode]}</p>
      </CardContent>
    </Card>
  );
}
