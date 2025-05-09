"use client";

import InfoCard from "@/app/components/info-card";
import { Typography } from "@/app/components/typography";
import { GET_ALL_MARKETS } from "@/app/plutus/queries";
import { useQuery } from "@urql/next";
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
    const [{ data }] = useQuery({ query: GET_ALL_MARKETS })

    return (
        <div {...props}>
            <Typography variant="h1" className="mb-10">{label}</Typography>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {data?.allMarkets?.nodes?.map(market => (
                    <Link href={`${href}/${market.id}`} key={market.id} >
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