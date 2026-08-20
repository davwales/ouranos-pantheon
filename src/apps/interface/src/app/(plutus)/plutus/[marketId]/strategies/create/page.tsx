"use client";

import { useParams } from "next/navigation";
import { StrategyConfigForm } from "../_components/strategy-config-form";

export default function CreateStrategyPage() {
  const { marketId } = useParams<{ marketId: string }>();

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-semibold tracking-tight">
        Create New Strategy
      </h2>
      <StrategyConfigForm mode="create" marketId={marketId} />
    </div>
  );
}
