import { Button } from "@/components/ui/button";
import { MessageSquarePlus, FolderPlus } from "lucide-react";
import Link from "next/link";

export function FolderEmptyState({
  folderName,
  onCreateSubfolder,
}: {
  folderName?: string;
  onCreateSubfolder?: () => void;
}) {
  return (
    <div className="flex flex-col items-center justify-center gap-4 py-12">
      <MessageSquarePlus className="h-10 w-10 text-muted-foreground" />
      <p className="text-sm text-muted-foreground">
        {folderName ? `No conversations in "${folderName}".` : "No conversations in this folder."}
      </p>
      <div className="flex items-center gap-2">
        <Button asChild variant="outline">
          <Link href="/hermes/chat">Start a Conversation</Link>
        </Button>
        {onCreateSubfolder && (
          <Button variant="outline" onClick={onCreateSubfolder}>
            <FolderPlus className="h-4 w-4 mr-2" />
            New Subfolder
          </Button>
        )}
      </div>
    </div>
  );
}
