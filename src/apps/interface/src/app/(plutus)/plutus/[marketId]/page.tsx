import { InfoCard } from "@/components/shared/info-card";
import { MarketAnalystActions } from "@/app/(plutus)/plutus/[marketId]/_components/market-analyst-actions";
import MarketOverview from "@/app/(plutus)/plutus/[marketId]/_components/market-overview";
import VolumeHeatmap from "@/app/(plutus)/plutus/[marketId]/_components/volume-heatmap";
import { MARKET_FEATURES } from "@/app/(plutus)/plutus/constants/market-features";
import Link from "next/link";

export default async function MarketLanding({
  params,
}: {
  params: Promise<{ marketId: string }>;
}) {
  const { marketId } = await params;

  return (
    <div className="space-y-6">
      <MarketAnalystActions marketId={marketId} />

      <MarketOverview marketId={marketId} />

      <VolumeHeatmap marketId={marketId} />

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {MARKET_FEATURES.map((feature, i) => (
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