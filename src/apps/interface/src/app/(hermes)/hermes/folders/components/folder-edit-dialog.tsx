"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Pencil, AlertTriangle } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

import type { ConversationFolder } from "../../conversations/components/folder-types";

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
  const [name, setName] = useState("");
  const [isPublic, setIsPublic] = useState(true);
  const [parentFolderId, setParentFolderId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (folder) {
      setName(folder.name);
      setIsPublic(folder.isPublic);
      setParentFolderId(folder.parentFolderId);
      setError(null);
    }
  }, [folder]);

  const handleSubmit = async () => {
    if (!name.trim()) {
      setError("Name is required");
      return;
    }

    setError(null);
    setIsSubmitting(true);
    try {
      const payload = {
        name: name.trim(),
        isPublic,
        parentFolderId,
      };
      await onSubmit(payload);
      onOpenChange(false);
    } catch {
      setError("Failed to update folder");
    } finally {
      setIsSubmitting(false);
    }
  };

  const selectedParent = useMemo(
    () => allFolders.find((f) => f.id === parentFolderId) ?? null,
    [allFolders, parentFolderId],
  );

  const InheritedWarning = () => {
    if (!selectedParent || !folder) return null;

    if (selectedParent.isPublic) return null;

    if (!folder.isPublic) return null;

    return (
      <div className="rounded-md bg-destructive/10 p-3 text-destructive border border-destructive/50">
        <div className="flex items-start gap-2">
          <AlertTriangle className="h-4 w-4 text-destructive shrink-0 mt-0.5" />
          <div className="text-sm space-y-1">
            <p className="font-medium">
              This folder will become private because its parent &apos;{selectedParent.name}&apos; is private.
            </p>
          </div>
        </div>
      </div>
    );
  };

  const descendantIds = new Set<string>();
  const collectDescendants = (f: ConversationFolder) => {
    f.children.forEach((c) => {
      descendantIds.add(c.id);
      collectDescendants(c);
    });
  };
  if (folder) {
    collectDescendants(folder);
  }

  const eligibleParents = allFolders.filter(
    (f) => f.id !== folder?.id && !descendantIds.has(f.id),
  );

  return (
    <ResponsiveDialog
      title="Edit Folder"
      description="Update folder details."
      open={open}
      onOpenChange={onOpenChange}
      trigger={<span className="hidden" />}
    >
      <div className="space-y-4">
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
            id="editIsPublic"
            checked={isPublic}
            onChange={(e) => setIsPublic(e.target.checked)}
            className="h-4 w-4 cursor-pointer"
          />
          <label htmlFor="editIsPublic" className="text-sm font-medium">
            Public
          </label>
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium">Parent Folder</label>
          <Select
            value={parentFolderId ?? "__root__"}
            onValueChange={(v) => setParentFolderId(v === "__root__" ? null : v)}
          >
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="__root__">(None)</SelectItem>
              {eligibleParents.map((f) => (
                <SelectItem key={f.id} value={f.id}>
                  {"\u00A0".repeat(f.depth * 2)}{f.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <InheritedWarning />
        </div>

        {error && <p className="text-sm text-destructive">{error}</p>}

        <Button
          onClick={handleSubmit}
          disabled={isSubmitting || !name.trim()}
          className="w-full"
        >
          <Pencil className="h-4 w-4 mr-2" />
          {isSubmitting ? "Saving..." : "Save Changes"}
        </Button>
      </div>
    </ResponsiveDialog>
  );
}
