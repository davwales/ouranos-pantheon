import { notFound } from "next/navigation";
import { BreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { plutusApi } from "@/lib/api/plutus";
import { ApiError } from "@/lib/api-client";

export default async function MarketLayout({
  children,
  params,
}: {
  children: React.ReactNode;
  params: Promise<{ marketId: string }>;
}) {
  const { marketId } = await params;

  let market;
  try {
    market = await plutusApi.getMarket(marketId);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      notFound();
    }
    throw error;
  }

  return (
    <>
      <BreadcrumbLabel segment={marketId} label={market.name} />
      {children}
    </>
  );
}

export const dynamic = "force-dynamic";
