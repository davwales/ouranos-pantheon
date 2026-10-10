import ClipboardCopy from "@/components/shared/clipboard-copy";
import { PrettyNumber } from "@/components/shared/pretty-number/pretty-number";
import { Typography } from "@/components/shared/typography";
import { type GetSymbolTradesResponse, type Symbol } from "@/lib/api/plutus";
import React, { type ReactNode } from "react";

function StatDisplay({
  label,
  children,
}: {
  label: string;
  children: ReactNode;
}): ReactNode {
  return (
    <div className="flex justify-between items-end gap-2">
      <Typography variant="h4">{label}</Typography>
      <span className="whitespace-nowrap">{children}</span>
    </div>
  );
}

export function SymbolStatsGrid({
  symbol,
  trades,
  ...props
}: React.ComponentProps<"div"> & {
  symbol?: Symbol;
  trades?: GetSymbolTradesResponse;
}) {
  return (
    <div {...props}>
      <StatDisplay label="Code">{symbol?.code}</StatDisplay>
      {symbol?.subcode && (
        <StatDisplay label="Subcode">{symbol.subcode}</StatDisplay>
      )}
      <StatDisplay label="Total Spent">
        <PrettyNumber number={trades?.totalSpent ?? 0} />
      </StatDisplay>
      <StatDisplay label="Minimum Price">
        <ClipboardCopy value={trades?.minPrice ?? 0}>
          <PrettyNumber number={trades?.minPrice ?? 0} />
        </ClipboardCopy>
      </StatDisplay>
      <StatDisplay label="Average Price">
        <ClipboardCopy value={trades?.averagePrice ?? 0}>
          <PrettyNumber number={trades?.averagePrice ?? 0} />
        </ClipboardCopy>
      </StatDisplay>
      <StatDisplay label="Maximum Price">
        <ClipboardCopy value={trades?.maxPrice ?? 0}>
          <PrettyNumber number={trades?.maxPrice ?? 0} />
        </ClipboardCopy>
      </StatDisplay>
      <StatDisplay label="Volume">
        <PrettyNumber number={trades?.volume ?? 0} />
      </StatDisplay>
      <StatDisplay label="# Transactions">
        <PrettyNumber number={trades?.numTransactions || 0} decimals={0} />
      </StatDisplay>
    </div>
  );
}
