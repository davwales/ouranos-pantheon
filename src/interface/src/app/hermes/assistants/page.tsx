"use client";

import InfoCard from "@/app/components/info-card";
import { GET_ASSISTANT_LIST } from "@/app/hermes/queries";
import { Button } from "@/components/ui/button";
import { useQuery } from "@urql/next";
import Link from "next/link";

export default function AssistantsPage() {
  const [{ data }] = useQuery({ query: GET_ASSISTANT_LIST });

  return (
    <div className="m-4">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {data?.allAssistants.map((assistant) => (
          <Link href={`/hermes/assistants/${assistant.id}`} key={assistant.id}>
            <InfoCard
              label={assistant.assistantName}
              description={assistant.model}
              className="hover:bg-accent h-full w-full"
            />
          </Link>
        ))}
      </div>

      <Button size="lg" variant="link" className="mt-4 w-full">
        <Link href="/hermes/assistants/create">Create New</Link>
      </Button>
    </div>
  );
}
