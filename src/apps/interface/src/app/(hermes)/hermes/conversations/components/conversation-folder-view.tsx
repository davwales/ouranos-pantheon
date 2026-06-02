"use client";

import { InfoCardGridSkeleton } from "@/components/shared/skeletons/info-card-skeleton";
import { useMemo } from "react";

import type { ConversationSummary } from "@/lib/api/hermes-types";
import { FolderEmptyState } from "../../folders/components/folder-empty-state";
import { ConversationCard } from "./conversation-card";
import { FolderCard } from "./folder-card";
import type { ConversationFolder } from "./folder-types";

export function ConversationFolderView({
  isLoading,
  folderTree,
  allFolders,
  conversations,
  currentFolderId,
  onNavigate,
  onCreateFolder,
  onEditFolder,
  onDeleteFolder,
  onDropConversation,
}: {
  isLoading: boolean;
  folderTree: ConversationFolder[];
  allFolders: ConversationFolder[];
  conversations: ConversationSummary[] | undefined;
  currentFolderId: string | null;
  onNavigate: (folderId: string) => void;
  onCreateFolder: (parentFolderId?: string) => void;
  onEditFolder: (folder: ConversationFolder) => void;
  onDeleteFolder: (folder: ConversationFolder) => void;
  onDropConversation: (
    conversationId: string,
    folderId: string | null,
  ) => Promise<void>;
}) {
  const currentFolder = useMemo(
    () => allFolders.find((f) => f.id === currentFolderId) ?? null,
    [allFolders, currentFolderId],
  );

  const childFolders = useMemo(() => {
    if (!currentFolderId) {
      return folderTree;
    }
    return currentFolder?.children ?? [];
  }, [currentFolderId, folderTree, currentFolder]);

  const childConversations = useMemo(() => {
    if (!currentFolderId) {
      return (conversations ?? []).filter((c) => c.folderId === null);
    }
    return (conversations ?? []).filter((c) => c.folderId === currentFolderId);
  }, [conversations, currentFolderId]);

  if (isLoading) {
    return <InfoCardGridSkeleton />;
  }

  return (
    <>
      {childFolders.length === 0 && childConversations.length === 0 ? (
        <FolderEmptyState
          folderName={currentFolder?.name}
          onCreateSubfolder={
            currentFolderId ? () => onCreateFolder(currentFolderId) : undefined
          }
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {childFolders.map((folder) => (
            <FolderCard
              key={folder.id}
              folder={folder}
              conversationCount={folder.conversationCount}
              subfolderCount={folder.subfolderCount}
              onClick={() => onNavigate(folder.id)}
              onCreateSubfolder={() => onCreateFolder(folder.id)}
              onEdit={() => onEditFolder(folder)}
              onDelete={() => onDeleteFolder(folder)}
              onDropConversation={(conversationId) =>
                onDropConversation(conversationId, folder.id)
              }
            />
          ))}
          {childConversations.map((conversation) => (
            <ConversationCard
              key={conversation.id}
              conversation={conversation}
              folders={allFolders}
              currentFolderId={currentFolderId}
              onMove={async (folderId) =>
                onDropConversation(conversation.id, folderId)
              }
            />
          ))}
        </div>
      )}
    </>
  );
}
