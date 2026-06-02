"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { FolderPlus, AlertTriangle } from "lucide-react";
import { useEffect, useState } from "react";

import type { ConversationFolder } from "../../conversations/components/folder-types";

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
  const isControlled = externalOpen !== undefined;
  const isOpen = isControlled ? externalOpen : internalOpen;
  const handleOpenChange = isControlled ? externalOnOpenChange! : setInternalOpen;
  const [name, setName] = useState("");
  const [isPublic, setIsPublic] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setName("");
      setIsPublic(parentFolder ? parentFolder.isPublic : true);
      setError(null);
    }
  }, [isOpen, parentFolder]);

  const InheritedWarning = () => {
    if (!parentFolder || parentFolder.isPublic) {
      return null;
    }

    return (
      <div className="rounded-md bg-destructive/10 p-3 text-destructive border border-destructive/50">
        <div className="flex items-start gap-2">
          <AlertTriangle className="h-4 w-4 text-destructive shrink-0 mt-0.5" />
          <div className="text-sm space-y-1">
            <p className="font-medium">
              This folder will be private because its parent &apos;{parentFolder.name}&apos; is private.
            </p>
            <p className="text-xs text-destructive">
              You can only change this on the parent folder.
            </p>
          </div>
        </div>
      </div>
    );
  };

  const handleSubmit = async () => {
    if (!name.trim()) {
      setError("Name is required");
      return;
    }
    setError(null);
    setIsSubmitting(true);
    try {
      await onSubmit({
        name: name.trim(),
        isPublic,
        parentFolderId: parentFolderId ?? null,
      });
      setName("");
      setIsPublic(true);
      handleOpenChange(false);
    } catch {
      setError("Failed to create folder");
    } finally {
      setIsSubmitting(false);
    }
  };

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
      <div className="space-y-4">
        <InheritedWarning />

        <div className="space-y-2">
          <label className="text-sm font-medium">Name</label>
          <Input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Folder name"
          />
        </div>

        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            id="isPublic"
            checked={isPublic}
            onChange={(e) => setIsPublic(e.target.checked)}
            className="h-4 w-4 cursor-pointer"
          />
          <label htmlFor="isPublic" className="text-sm font-medium">
            Public
          </label>
        </div>

        {error && <p className="text-sm text-destructive">{error}</p>}

        <Button
          onClick={handleSubmit}
          disabled={isSubmitting || !name.trim()}
          className="w-full"
        >
          {isSubmitting ? "Creating..." : "Create Folder"}
        </Button>
      </div>
    </ResponsiveDialog>
  );
}
