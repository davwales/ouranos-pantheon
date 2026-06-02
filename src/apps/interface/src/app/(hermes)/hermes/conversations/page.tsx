"use client";

import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import type { MessageInput } from "@/lib/api/hermes-types";
import { useRouter, useSearchParams } from "next/navigation";
import { useCallback, useMemo, useState } from "react";

import { FolderCreateDialog } from "../folders/components/folder-create-dialog";
import { FolderEditDialog } from "../folders/components/folder-edit-dialog";

import { ConversationFolderView } from "./components/conversation-folder-view";
import { ConversationToolbar } from "./components/conversation-toolbar";
import { FolderErrorBanner } from "./components/folder-error-banner";
import type { ConversationFolder } from "./components/folder-types";
import { useMoveConversation } from "./lib/use-move-conversation";
import { buildFolderTree, flattenFolders } from "./lib/folder-utils";

export default function ConversationsPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const currentFolderId = searchParams.get("folder") || null;

  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editingFolder, setEditingFolder] = useState<ConversationFolder | null>(
    null,
  );
  const [createParentId, setCreateParentId] = useState<string | undefined>(
    undefined,
  );
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [foldersRefreshKey, setFoldersRefreshKey] = useState(0);
  const [conversationsRefreshKey, setConversationsRefreshKey] = useState(0);
  const [foldersState] = useApi(
    () => hermesApi.getAllFolders(),
    [foldersRefreshKey],
  );

  const [conversationsState] = useApi(
    () =>
      hermesApi.getAllConversations(
        currentFolderId
          ? { folderId: currentFolderId }
          : undefined,
      ),
    [currentFolderId, conversationsRefreshKey],
  );

  const folderTree = useMemo(
    () => buildFolderTree(foldersState.data ?? []),
    [foldersState.data],
  );

  const allFolders = useMemo(
    () => flattenFolders(folderTree),
    [folderTree],
  );

  const handleNavigate = useCallback(
    (folderId: string | null) => {
      if (!folderId) {
        router.push("/hermes/conversations");
        return;
      }
      router.push(`/hermes/conversations?folder=${folderId}`);
    },
    [router],
  );

  const handleDropConversation = useMoveConversation({
    onSuccess: () => {
      setFoldersRefreshKey((k) => k + 1);
      setConversationsRefreshKey((k) => k + 1);
    },
  });

  const handleCreateFolder = useCallback(
    (parentFolderId?: string) => {
      setCreateParentId(parentFolderId ?? currentFolderId ?? undefined);
      setCreateDialogOpen(true);
    },
    [currentFolderId],
  );

  const handleSubmitCreate = useCallback(
    async (input: {
      name: string;
      isPublic: boolean;
      parentFolderId?: string | null;
    }) => {
      await hermesApi.createFolder(input);
      setFoldersRefreshKey((k) => k + 1);
      setCreateDialogOpen(false);
    },
    [],
  );

  const handleEditFolder = useCallback((folder: ConversationFolder) => {
    setEditingFolder(folder);
    setEditDialogOpen(true);
  }, []);

  const handleSubmitEdit = useCallback(
    async (input: {
      name: string;
      isPublic: boolean;
      parentFolderId?: string | null;
    }) => {
      if (!editingFolder) return;
      await hermesApi.updateFolder(editingFolder.id, input);
      setFoldersRefreshKey((k) => k + 1);
    },
    [editingFolder],
  );

  const handleDeleteFolder = useCallback(
    async (folder: ConversationFolder) => {
      try {
        setDeleteError(null);
        await hermesApi.deleteFolder(folder.id);
        setFoldersRefreshKey((k) => k + 1);
        if (currentFolderId === folder.id) {
          router.push("/hermes/conversations");
        }
      } catch {
        setDeleteError("Failed to delete folder. Please try again.");
      }
    },
    [currentFolderId, router],
  );

  const isLoading =
    (!foldersState.data && foldersState.status === "loading") ||
    (!conversationsState.data && conversationsState.status === "loading");

  return (
    <div className="m-4 space-y-4">
      <ConversationToolbar
        currentFolderId={currentFolderId}
        allFolders={allFolders}
        onNavigate={handleNavigate}
      >
        <FolderCreateDialog
          onSubmit={handleSubmitCreate}
          parentFolderId={createParentId}
          parentFolder={allFolders.find((f) => f.id === createParentId)}
          open={createDialogOpen}
          onOpenChange={(open) => {
            if (open) {
              setCreateParentId(currentFolderId ?? undefined);
            }
            setCreateDialogOpen(open);
          }}
        />
      </ConversationToolbar>

      {deleteError && <FolderErrorBanner message={deleteError} />}

      {foldersState.status === "error" && (
        <FolderErrorBanner
          message="Failed to load folders"
          onRetry={() => setFoldersRefreshKey((k) => k + 1)}
        />
      )}

      {conversationsState.status === "error" && (
        <FolderErrorBanner
          message="Failed to load conversations"
          onRetry={() => setConversationsRefreshKey((k) => k + 1)}
        />
      )}

      <ConversationFolderView
        isLoading={isLoading}
        folderTree={folderTree}
        allFolders={allFolders}
        conversations={conversationsState.data}
        currentFolderId={currentFolderId}
        onNavigate={handleNavigate}
        onCreateFolder={handleCreateFolder}
        onEditFolder={handleEditFolder}
        onDeleteFolder={handleDeleteFolder}
        onDropConversation={handleDropConversation}
      />

      <FolderEditDialog
        folder={editingFolder}
        allFolders={allFolders}
        onSubmit={handleSubmitEdit}
        open={editDialogOpen}
        onOpenChange={setEditDialogOpen}
      />
    </div>
  );
}
