"use client";

import { InfoCard } from "@/components/shared/info-card";
import { InfoCardGridSkeleton } from "@/components/shared/skeletons/info-card-skeleton";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import Link from "next/link";

export default function LorasPage() {
  const [state] = useApi(() => hermesApi.getAllLoras());

  return (
    <div className="m-4">
      {state.status === "loading" && !state.data ? (
        <InfoCardGridSkeleton />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {state.data?.map((lora) => (
            <Link href={`/hermes/loras/${lora.id}`} key={lora.id}>
              <InfoCard
                label={lora.name}
                description={`${lora.identifier} · weight ${lora.weight}`}
                className="hover:bg-accent h-full w-full"
              />
            </Link>
          ))}
        </div>
      )}

      <Button size="lg" variant="link" className="mt-4 w-full">
        <Link href="/hermes/loras/create">Create New</Link>
      </Button>
    </div>
  );
}
