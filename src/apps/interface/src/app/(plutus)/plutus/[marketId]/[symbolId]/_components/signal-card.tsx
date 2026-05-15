import { Typography } from "@/components/shared/typography";
import { SignalResponse } from "@/lib/api/plutus";

function ValueBar({ value }: { value: number }) {
  const isPositive = value > 0;
  const fillStart = isPositive ? 50 : ((value + 1) / 2) * 100;
  const fillWidth = (Math.abs(value) / 2) * 100;
  const fillColor = isPositive
    ? "bg-green-500"
    : value < 0
      ? "bg-red-500"
      : "bg-muted-foreground";

  return (
    <div>
      <div className="relative h-2 bg-muted rounded-full overflow-hidden">
        <div className="absolute inset-y-0 left-1/2 w-px bg-border z-10" />
        <div
          className={`absolute inset-y-0 rounded-full ${fillColor}`}
          style={{ left: `${fillStart}%`, width: `${fillWidth}%` }}
        />
      </div>
      <div className="flex justify-between mt-1">
        <Typography variant="muted" className="text-xs">
          -1
        </Typography>
        <Typography
          variant="small"
          className={`font-mono font-semibold ${
            isPositive
              ? "text-green-600"
              : value < 0
                ? "text-red-500"
                : "text-muted-foreground"
          }`}
        >
          {value >= 0 ? "+" : ""}
          {value.toFixed(2)}
        </Typography>
        <Typography variant="muted" className="text-xs">
          +1
        </Typography>
      </div>
    </div>
  );
}

export function SignalCard({ signal }: { signal: SignalResponse }) {
  const { label, description, value, direction, strength, intents } = signal;

  const directionColor =
    direction === "Bullish"
      ? "text-green-600"
      : direction === "Bearish"
        ? "text-red-500"
        : "text-muted-foreground";

  return (
    <div className="border rounded-lg p-4 flex flex-col gap-3">
      <div>
        <Typography variant="h4">{label}</Typography>
        <Typography variant="muted" className="text-sm leading-snug mt-0.5">
          {description}
        </Typography>
      </div>

      <ValueBar value={value} />

      <div className="flex items-center justify-between gap-2 flex-wrap">
        <div className="flex gap-1 flex-wrap">
          {intents.map((intent) => (
            <span
              key={intent}
              className="text-xs px-2 py-0.5 rounded-full bg-muted text-muted-foreground border"
            >
              {intent}
            </span>
          ))}
        </div>
        <Typography variant="small" className={directionColor}>
          {strength} · {direction}
        </Typography>
      </div>
    </div>
  );
}
