"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog";

import type { ConversationFolder } from "../../conversations/components/folder-types";
import { FolderEditForm } from "./folder-edit-form";

export function FolderEditDialog({
  folder,
  allFolders,
  onSubmit,
  open,
  onOpenChange,
}: {
  folder: ConversationFolder | null;
  allFolders: ConversationFolder[];
  onSubmit: (input: {
    name: string;
    isPublic: boolean;
    parentFolderId?: string | null;
  }) => Promise<void>;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  return (
    <ResponsiveDialog
      title="Edit Folder"
      description="Update folder details."
      open={open}
      onOpenChange={onOpenChange}
      trigger={<span className="hidden" />}
    >
      {folder && (
        <FolderEditForm
          key={folder.id}
          folder={folder}
          allFolders={allFolders}
          onSubmit={onSubmit}
          onDone={() => onOpenChange(false)}
        />
      )}
    </ResponsiveDialog>
  );
}
