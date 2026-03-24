"use client";

import InfoCard from "@/app/components/info-card";
import { Typography } from "@/app/components/typography";
import { useApi } from "@/hooks/use-api";
import { plutusApi } from "@/lib/api/plutus";
import Link from "next/link";
import React from "react";

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
    </div>
  );
}
