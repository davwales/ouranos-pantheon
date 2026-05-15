"use client";

// Views separate UI composition (Client Components) from data-fetching Server Components (page.tsx).
// This pattern keeps page.tsx focused on data fetching and passing props down.

import InfoCard from "@/components/shared/info-card";
import { Typography } from "@/components/shared/typography";
import { useApi } from "@/hooks/use-api";
import { plutusApi } from "@/lib/api/plutus";
import Link from "next/link";
import React from "react";
import { InfoCardGridSkeleton } from "@/components/shared/skeletons/info-card-skeleton";

export default function MarketSelectionView({
  label,
  href,
  ...props
}: React.ComponentProps<"div"> & {
  label: string;
  href: string;
}) {
  const [state] = useApi(() => plutusApi.getAllMarkets());

  return (
    <div {...props}>
      <Typography variant="h1" className="mb-10">
        {label}
      </Typography>
      {state.status === "loading" && !state.data ? (
        <InfoCardGridSkeleton count={3} hasIcon />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {state.data?.map((market) => (
            <Link href={`${href}/${market.id}`} key={market.id}>
              <InfoCard
                label={market.name}
                description={market.description}
                iconSrc={market.icon}
                className="hover:bg-accent h-full w-full"
              />
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
