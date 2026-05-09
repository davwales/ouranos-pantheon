import { type PositionStatus } from "@/lib/api/plutus";

export const positionSideLabels: Record<string, string> = {
  Buy: "Buy",
  Sell: "Sell",
};

export const positionSideColors: Record<string, string> = {
  Buy: "bg-sky-100 text-sky-800",
  Sell: "bg-violet-100 text-violet-800",
};

export const positionStatusLabels: Record<string, string> = {
  Pending: "Pending",
  DidNotBuy: "Did Not Buy",
  Bought: "Bought",
  DidNotSell: "Did Not Sell",
  Sold: "Sold",
};

export const positionStatusColors: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-800",
  DidNotBuy: "bg-gray-100 text-gray-800",
  Bought: "bg-green-100 text-green-800",
  DidNotSell: "bg-gray-100 text-gray-800",
  Sold: "bg-blue-100 text-blue-800",
};

export const buyCloseStatuses: PositionStatus[] = ["DidNotBuy", "Bought"];
export const sellCloseStatuses: PositionStatus[] = ["DidNotSell", "Sold"];
