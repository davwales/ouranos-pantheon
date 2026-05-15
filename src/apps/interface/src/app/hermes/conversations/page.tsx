"use client";

import InfoCard from "@/app/components/info-card";
import { InfoCardGridSkeleton } from "@/app/components/skeletons/info-card-skeleton";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import Link from "next/link";

export default function ConversationsPage() {
  const [state] = useApi(() => hermesApi.getAllConversations());

  return (
    <div className="m-4">
      {state.status === "loading" && !state.data ? (
        <InfoCardGridSkeleton />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {state.data?.map((conversation) => (
            <Link
              key={conversation.id}
              href={`/hermes/conversations/${conversation.id}`}
            >
              <InfoCard
                label={conversation.name}
                description={new Date(
                  conversation.updatedAt,
                ).toLocaleDateString()}
                className="hover:bg-accent h-full w-full"
              />
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}