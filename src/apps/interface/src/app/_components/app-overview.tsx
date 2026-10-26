import { InfoCard } from "@/components/shared/info-card";
import { Typography } from "@/components/shared/typography";
import Link from "next/link";

const MODULES: { label: string; description: string; href: string }[] = [
  {
    label: "Hermes",
    description:
      "AI-powered conversations with customizable personas and models.",
    href: "/hermes/chat",
  },
  {
    label: "Plutus",
    description: "Market data aggregation, analysis, and forecasting tools.",
    href: "/plutus",
  },
  {
    label: "Hestia",
    description: "Saved recipes, recipe imports, and a shared shopping list.",
    href: "/hestia/recipes",
  },
];

export function AppOverview() {
  return (
    <div className="space-y-4">
      <Typography variant="h1">Ouranos Pantheon</Typography>
      <Typography variant="muted">
        A modular platform combining AI-powered conversations, market data
        analytics, and kitchen management.
      </Typography>
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        {MODULES.map((appModule) => (
          <Link key={appModule.href} href={appModule.href}>
            <InfoCard
              label={appModule.label}
              description={appModule.description}
              className="h-full w-full hover:bg-accent"
            />
          </Link>
        ))}
      </div>
    </div>
  );
}
