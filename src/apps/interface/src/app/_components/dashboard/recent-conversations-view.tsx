"use client";

import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { formatDistanceToNow } from "date-fns";
import { MessagesSquare } from "lucide-react";
import { DashboardCard } from "./dashboard-card";
import { DashboardRow } from "./dashboard-row";

const CONVERSATIONS_SHOWN = 5;

export function RecentConversationsView() {
  const [state] = useApi(
    async () =>
      (
        await hermesApi.getAllConversations({
          sortField: "UpdatedAt",
          sortDirection: "desc",
        })
      ).slice(0, CONVERSATIONS_SHOWN),
    [],
  );

  return (
    <DashboardCard
      title="Recent Chats"
      icon={MessagesSquare}
      href="/hermes/conversations"
      state={state}
      isEmpty={(data) => data.length === 0}
      emptyText="No saved conversations yet."
    >
      {(data) => (
        <ul>
          {data.map((conversation) => (
            <DashboardRow
              key={conversation.id}
              href={`/hermes/conversations/${conversation.id}`}
              primary={conversation.name}
              secondary={`Updated ${formatDistanceToNow(new Date(conversation.updatedAt), { addSuffix: true })}`}
            />
          ))}
        </ul>
      )}
    </DashboardCard>
  );
}
