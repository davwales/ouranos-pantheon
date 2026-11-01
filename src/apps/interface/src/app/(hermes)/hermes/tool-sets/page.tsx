"use client";

import { InfoCard } from "@/components/shared/info-card";
import { InfoCardGridSkeleton } from "@/components/shared/skeletons/info-card-skeleton";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { hermesApi, type ToolSetSummary } from "@/lib/api/hermes";
import Link from "next/link";
import { usePublicMode } from "@/app/(hermes)/hermes/components/public-mode-context";

function describeToolSet(toolSet: ToolSetSummary): string {
  const tools =
    toolSet.tools.length === 0
      ? "No tools"
      : toolSet.tools.map((t) => t.name).join(", ");
  return toolSet.description ? `${toolSet.description} · ${tools}` : tools;
}

export default function ToolSetsPage() {
  const [state] = useApi(() => hermesApi.getAllToolSets());
  const isPublicMode = usePublicMode();

  return (
    <div className="m-4">
      {state.status === "loading" && !state.data ? (
        <InfoCardGridSkeleton />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {state.data?.map((toolSet) => (
            <Link href={`/hermes/tool-sets/${toolSet.id}`} key={toolSet.id}>
              <InfoCard
                label={toolSet.name}
                description={describeToolSet(toolSet)}
                className="hover:bg-accent h-full w-full"
              />
            </Link>
          ))}
        </div>
      )}

      {!isPublicMode && (
        <Button size="lg" variant="link" className="mt-4 w-full">
          <Link href="/hermes/tool-sets/create">Create New</Link>
        </Button>
      )}
    </div>
  );
}
