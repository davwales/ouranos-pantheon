"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog";
import { Button } from "@/components/ui/button";
import { AlertTriangle } from "lucide-react";
import { useState } from "react";

export function FolderDeleteButton({
  onConfirm,
  trigger,
}: {
  onConfirm: () => Promise<void>;
  trigger: React.ReactNode;
}) {
  const [open, setOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleDelete = async () => {
    setError(null);
    setIsDeleting(true);
    try {
      await onConfirm();
      setOpen(false);
    } catch {
      setError("Failed to delete folder");
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <ResponsiveDialog
      title="Delete Folder"
      description="This action cannot be undone. All nested folders and conversations inside will also be removed."
      open={open}
      onOpenChange={setOpen}
      trigger={trigger}
    >
      <div className="space-y-4">
        <div className="flex items-center gap-3 rounded-md bg-destructive/10 p-3 border border-destructive/50">
          <AlertTriangle className="h-5 w-5 text-destructive shrink-0" />
          <p className="text-sm text-destructive">
            Deleting this folder will permanently remove all nested folders and conversations.
          </p>
        </div>

        {error && <p className="text-sm text-destructive">{error}</p>}

        <Button
          variant="destructive"
          onClick={handleDelete}
          disabled={isDeleting}
          className="w-full"
        >
          {isDeleting ? "Deleting..." : "Delete Folder"}
        </Button>
      </div>
    </ResponsiveDialog>
  );
}
