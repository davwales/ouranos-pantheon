import { type PositionLimit } from "@/lib/api/plutus-types";

export const POSITION_LIMIT_LABELS: Record<PositionLimit, string> = {
  Budget: "Budget",
  Liquidity: "Liquidity",
  BuyLimit: "Buy limit",
};
