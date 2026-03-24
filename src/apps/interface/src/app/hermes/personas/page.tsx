"use client";

import InfoCard from "@/app/components/info-card";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import Link from "next/link";

export default function PersonasPage() {
  const [state] = useApi(() => hermesApi.getAllPersonas());

  return (
    <div className="m-4">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {state.data?.map((persona) => (
          <Link href={`/hermes/personas/${persona.id}`} key={persona.id}>
            <InfoCard
              label={persona.name}
              description={persona.description}
              className="hover:bg-accent h-full w-full"
            />
          </Link>
        ))}
      </div>

      <Button size="lg" variant="link" className="mt-4 w-full">
        <Link href="/hermes/personas/create">Create New</Link>
      </Button>
    </div>
  );
}
