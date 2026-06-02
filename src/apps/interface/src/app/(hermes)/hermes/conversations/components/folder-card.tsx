"use client";

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { FolderVisibilityBadge } from "@/app/(hermes)/hermes/folders/components/folder-visibility-badge";
import { FolderDeleteButton } from "@/app/(hermes)/hermes/folders/components/folder-delete-button";
import { cn } from "@/lib/utils";
import { Folder, MoreHorizontal, Pencil, Plus, Trash2 } from "lucide-react";
import { useState } from "react";

import type { ConversationFolder } from "./folder-types";

export function FolderCard({
  folder,
  conversationCount,
  subfolderCount,
  onClick,
  onCreateSubfolder,
  onEdit,
  onDelete,
  onDropConversation,
}: {
  folder: ConversationFolder;
  conversationCount: number;
  subfolderCount: number;
  onClick: () => void;
  onCreateSubfolder: () => void;
  onEdit: () => void;
  onDelete: () => void;
  onDropConversation?: (conversationId: string) => Promise<void>;
}) {
  const [isDragOver, setIsDragOver] = useState(false);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "move";
    setIsDragOver(true);
  };

  const handleDragLeave = () => {
    setIsDragOver(false);
  };

  const handleDrop = async (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
    const conversationId = e.dataTransfer.getData("text/plain");
    if (!conversationId || !onDropConversation) return;
    await onDropConversation(conversationId);
  };

  const hasContents = conversationCount > 0 || subfolderCount > 0;
  const subtitle = hasContents
    ? `${conversationCount} conversation${conversationCount !== 1 ? "s" : ""}, ${subfolderCount} folder${subfolderCount !== 1 ? "s" : ""}`
    : "Empty folder";

  return (
    <div
      onClick={onClick}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
      className={cn(
        "group relative rounded-4xl border-2 border-accent py-4 px-3 h-full w-full cursor-pointer transition-colors",
        "border-l-primary border-l-4",
        isDragOver ? "ring-2 ring-primary/50 bg-accent" : "hover:bg-accent/50",
      )}
    >
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-3 min-w-0">
          <div className="shrink-0 w-12 h-12 bg-primary/10 rounded-2xl flex items-center justify-center">
            <Folder className="h-6 w-6 text-primary" />
          </div>
          <div className="min-w-0">
            <p className="font-semibold text-sm truncate">{folder.name}</p>
            <p className="text-xs text-muted-foreground">{subtitle}</p>
          </div>
        </div>
        <div className="flex items-center gap-1 shrink-0">
          <FolderVisibilityBadge isPublic={folder.isPublic} />
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button
                className="p-1.5 rounded-md hover:bg-accent transition-colors"
                aria-label="Folder actions"
                onClick={(e) => e.stopPropagation()}
              >
                <MoreHorizontal className="h-4 w-4" />
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem
                onClick={(e) => {
                  e.stopPropagation();
                  onCreateSubfolder();
                }}
              >
                <Plus className="size-4 mr-2" /> New Subfolder
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={(e) => {
                  e.stopPropagation();
                  onEdit();
                }}
              >
                <Pencil className="size-4 mr-2" /> Edit
              </DropdownMenuItem>
              <FolderDeleteButton
                onConfirm={async () => onDelete()}
                trigger={
                  <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                    <Trash2 className="h-4 w-4 mr-2" /> Delete
                  </DropdownMenuItem>
                }
              />
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
    </div>
  );
}
