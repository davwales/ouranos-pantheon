"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog";
import { Button } from "@/components/ui/button";
import { FolderPlus } from "lucide-react";
import { useState } from "react";

import type { ConversationFolder } from "../../conversations/components/folder-types";
import { FolderCreateForm } from "./folder-create-form";

export function FolderCreateDialog({
  onSubmit,
  parentFolderId,
  parentFolder,
  open: externalOpen,
  onOpenChange: externalOnOpenChange,
}: {
  onSubmit: (input: {
    name: string;
    isPublic: boolean;
    parentFolderId?: string | null;
  }) => Promise<void>;
  parentFolderId?: string;
  parentFolder?: ConversationFolder;
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
}) {
  const [internalOpen, setInternalOpen] = useState(false);
  const isOpen = externalOpen ?? internalOpen;
  const handleOpenChange = externalOnOpenChange ?? setInternalOpen;

  return (
    <ResponsiveDialog
      title="Create Folder"
      description="Add a new folder to organize your conversations."
      open={isOpen}
      onOpenChange={handleOpenChange}
      trigger={
        <Button variant="outline" size="sm">
          <FolderPlus className="h-4 w-4 mr-2" />
          New Folder
        </Button>
      }
    >
      <FolderCreateForm
        onSubmit={onSubmit}
        onDone={() => handleOpenChange(false)}
        parentFolderId={parentFolderId}
        parentFolder={parentFolder}
      />
    </ResponsiveDialog>
  );
}
