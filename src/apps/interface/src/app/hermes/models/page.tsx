"use client";

import InfoCard from "@/app/components/info-card";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import Link from "next/link";

export default function ModelsPage() {
  const [state] = useApi(() => hermesApi.getAllModels());

  return (
    <div className="m-4">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {state.data?.map((model) => (
          <Link href={`/hermes/models/${model.id}`} key={model.id}>
            <InfoCard
              label={model.name}
              description={model.modelIdentifier}
              className="hover:bg-accent h-full w-full"
            />
          </Link>
        ))}
      </div>

      <Button size="lg" variant="link" className="mt-4 w-full">
        <Link href="/hermes/models/create">Create New</Link>
      </Button>
    </div>
  );
}
