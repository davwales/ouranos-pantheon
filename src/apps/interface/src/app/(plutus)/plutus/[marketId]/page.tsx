import { InfoCard } from "@/components/shared/info-card";
import MarketOverview from "@/app/(plutus)/plutus/[marketId]/_components/market-overview";
import VolumeHeatmap from "@/app/(plutus)/plutus/[marketId]/_components/volume-heatmap";
import Link from "next/link";

const features: {
  name: string;
  description: string;
  segment: string;
}[] = [
  {
    name: "Explorer",
    description:
      "Explore market data and gain insights into pricing trends to make informed investment decisions.",
    segment: "explorer",
  },
  {
    name: "Recipes",
    description:
      "Discover how you can combine different symbols to exploit a margin.",
    segment: "recipes",
  },
  {
    name: "Forecasts",
    description: "Get a glimpse of the future with our AI-powered forecasts.",
    segment: "forecasts",
  },
  {
    name: "Recent Trades",
    description:
      "View trades as they come in, gaining insights into the market before anyone else.",
    segment: "recent",
  },
  {
    name: "Signal Rankings",
    description:
      "See which symbols have the strongest signals across all investment intents.",
    segment: "signals",
  },
  {
    name: "Groups",
    description:
      "Organize symbols into custom groups like wishlists or categories to browse related items together.",
    segment: "groups",
  },
  {
    name: "Strategies",
    description:
      "Define and manage trading strategies. Configure parameters, toggle active status, and review past performance.",
    segment: "strategies",
  },
  {
    name: "Portfolio",
    description:
      "Track and manage your positions. Create, edit, close, and link buy and sell positions.",
    segment: "portfolio",
  },
];

export default async function MarketLanding({
  params,
}: {
  params: Promise<{ marketId: string }>;
}) {
  const { marketId } = await params;

  return (
    <div className="space-y-6">
      <MarketOverview marketId={marketId} />

      <VolumeHeatmap marketId={marketId} />

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {features.map((feature, i) => (
          <Link href={`/plutus/${marketId}/${feature.segment}`} key={i}>
            <InfoCard
              label={feature.name}
              description={feature.description}
              className="hover:bg-accent h-full w-full"
            />
          </Link>
        ))}
      </div>
    </div>
  );
}