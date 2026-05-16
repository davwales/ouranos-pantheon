import Link from "next/link";
import { InfoCard } from "@/components/shared/info-card";
import { Typography } from "@/components/shared/typography";

export function AppOverview() {
  return (
    <div className="space-y-4">
      <Typography variant="h1">Ouranos Pantheon</Typography>
      <Typography variant="muted">
        A modular platform combining AI-powered conversations and market data analytics.
      </Typography>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Link href="/hermes/chat">
          <InfoCard
            label="Hermes"
            description="AI-powered conversations with customizable personas and models."
            className="hover:bg-accent h-full w-full"
          />
        </Link>
        <Link href="/plutus">
          <InfoCard
            label="Plutus"
            description="Market data aggregation, analysis, and forecasting tools."
            className="hover:bg-accent h-full w-full"
          />
        </Link>
      </div>
    </div>
  );
}
