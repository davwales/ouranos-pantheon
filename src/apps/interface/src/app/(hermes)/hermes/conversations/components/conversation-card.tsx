"use client";

import { InfoCard } from "@/components/shared/info-card";
import { cn } from "@/lib/utils";
import { GripVertical, MoreHorizontal } from "lucide-react";
import Link from "next/link";

import { MoveToFolderDropdown } from "./move-to-folder-dropdown";
import type { ConversationFolder } from "./folder-types";
import type { ConversationSummary } from "@/lib/api/hermes-types";

export function ConversationCard({
  conversation,
  folders,
  currentFolderId,
  onMove,
}: {
  conversation: ConversationSummary;
  folders: ConversationFolder[];
  currentFolderId: string | null;
  onMove?: (folderId: string | null) => Promise<void>;
}) {
  return (
    <div
      draggable={Boolean(onMove)}
      onDragStart={(e) => {
        e.dataTransfer.setData("text/plain", conversation.id);
        e.dataTransfer.effectAllowed = "move";
      }}
      className={cn(
        "relative group",
        onMove && "cursor-grab active:cursor-grabbing",
      )}
    >
      <Link href={`/hermes/conversations/${conversation.id}`}>
        <InfoCard
          label={conversation.name}
          description={new Date(
            conversation.updatedAt,
          ).toLocaleDateString()}
          className={cn("hover:bg-accent h-full w-full", onMove && "pl-8")}
        />
      </Link>
      {onMove && (
        <div className="absolute left-2 top-1/2 -translate-y-1/2 opacity-50 group-hover:opacity-100 transition-opacity pointer-events-none">
          <GripVertical className="h-4 w-4 text-muted-foreground" />
        </div>
      )}
      {onMove && (
        <div className="absolute top-3 right-3 transition-opacity">
          <MoveToFolderDropdown
            folders={folders}
            currentFolderId={currentFolderId}
            onMove={onMove}
            trigger={
              <button
                className="p-1.5 rounded-md hover:bg-accent transition-colors"
                aria-label="Move conversation"
                onMouseDown={(e) => e.stopPropagation()}
              >
                <MoreHorizontal className="h-4 w-4" />
              </button>
            }
          />
        </div>
      )}
    </div>
  );
}
