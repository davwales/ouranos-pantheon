import { notFound } from "next/navigation";
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

  try {
    await plutusApi.getMarket(marketId);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      notFound();
    }
    throw error;
  }

  return <>{children}</>;
}

export const dynamic = "force-dynamic";
