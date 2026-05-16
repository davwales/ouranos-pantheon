"use client";

import { InfoCard } from "@/components/shared/info-card";
import { InfoCardGridSkeleton } from "@/components/shared/skeletons/info-card-skeleton";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import Link from "next/link";

export default function TraitsPage() {
  const [state] = useApi(() => hermesApi.getAllTraits());

  return (
    <div className="m-4">
      {state.status === "loading" && !state.data ? (
        <InfoCardGridSkeleton />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {state.data?.map((trait) => (
            <Link href={`/hermes/traits/${trait.id}`} key={trait.id}>
              <InfoCard
                label={trait.name}
                description={trait.content}
                className="hover:bg-accent h-full w-full"
              />
            </Link>
          ))}
        </div>
      )}

      <Button size="lg" variant="link" className="mt-4 w-full">
        <Link href="/hermes/traits/create">Create New</Link>
      </Button>
    </div>
  );
}