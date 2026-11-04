import { MoveToFolderDropdown } from "@/app/(hermes)/hermes/conversations/components/move-to-folder-dropdown";
import { type ConversationFolder } from "./folder-types";

type ConversationHeaderProps = {
  name: string;
  folders: ConversationFolder[];
  currentFolderId: string | null;
  onMove?: (folderId: string | null) => Promise<void>;
};

export function ConversationHeader({
  name,
  folders,
  currentFolderId,
  onMove,
}: ConversationHeaderProps) {
  return (
    <div className="flex items-center justify-between m-4 mb-0">
      <h1 className="text-lg font-semibold truncate">{name}</h1>
      {onMove && (
        <MoveToFolderDropdown
          folders={folders}
          currentFolderId={currentFolderId}
          onMove={onMove}
        />
      )}
    </div>
  );
}
