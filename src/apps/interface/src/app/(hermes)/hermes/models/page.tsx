"use client";

import InfoCard from "@/components/shared/info-card";
import { InfoCardGridSkeleton } from "@/components/shared/skeletons/info-card-skeleton";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { AlertTriangle } from "lucide-react";
import Link from "next/link";

export default function ModelsPage() {
  const [state] = useApi(() => hermesApi.getAllModels());

  return (
    <div className="m-4">
      {state.status === "loading" && !state.data ? (
        <InfoCardGridSkeleton />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {state.data?.map((model) => (
            <Link href={`/hermes/models/${model.id}`} key={model.id}>
              <InfoCard
                label={model.name}
                description={model.modelIdentifier}
                className="hover:bg-accent h-full w-full"
              >
                {model.isUnavailable && (
                  <Badge variant="destructive" className="mt-1 gap-1">
                    <AlertTriangle className="h-3 w-3" />
                    Unavailable
                  </Badge>
                )}
              </InfoCard>
            </Link>
          ))}
        </div>
      )}

      <Button size="lg" variant="link" className="mt-4 w-full">
        <Link href="/hermes/models/create">Create New</Link>
      </Button>
    </div>
  );
}