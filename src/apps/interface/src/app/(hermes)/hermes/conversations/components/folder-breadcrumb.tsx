"use client";

import { ChevronRight } from "lucide-react";

import type { ConversationFolder } from "./folder-types";

export function FolderBreadcrumb({
  currentFolderId,
  folders,
  onNavigate,
}: {
  currentFolderId: string | null;
  folders: ConversationFolder[];
  onNavigate: (folderId: string | null) => void;
}) {
  const path: ConversationFolder[] = [];

  let currentId = currentFolderId;
  while (currentId) {
    const folder = folders.find((f) => f.id === currentId);
    if (!folder) break;
    path.unshift(folder);
    currentId = folder.parentFolderId;
  }

  const isRoot = currentFolderId === null;

  return (
    <nav aria-label="Breadcrumb" className="flex items-center gap-1 text-sm">
      {isRoot ? (
        <span className="font-semibold" aria-current="page">Conversations</span>
      ) : (
        <button
          onClick={() => onNavigate(null)}
          className="text-muted-foreground hover:text-foreground transition-colors"
        >
          Conversations
        </button>
      )}

      {path.map((folder, index) => {
        const isLast = index === path.length - 1;
        return (
          <span key={folder.id} className="flex items-center gap-1">
            <ChevronRight className="h-3.5 w-3.5 text-muted-foreground" />
            {isLast ? (
              <span className="font-semibold" aria-current="page">{folder.name}</span>
            ) : (
              <button
                onClick={() => onNavigate(folder.id)}
                className="text-muted-foreground hover:text-foreground transition-colors"
              >
                {folder.name}
              </button>
            )}
          </span>
        );
      })}
    </nav>
  );
}
