import { type PositionLimit } from "@/lib/api/plutus-types";

export const POSITION_LIMIT_LABELS: Record<PositionLimit, string> = {
  Budget: "budget",
  Liquidity: "liquidity",
  BuyLimit: "buy limit",
};
