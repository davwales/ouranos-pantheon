"use client";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { FolderInput } from "lucide-react";
import { useState } from "react";

import type { ConversationFolder } from "./folder-types";

export function MoveToFolderDropdown({
  folders,
  currentFolderId,
  onMove,
  trigger,
}: {
  folders: ConversationFolder[];
  currentFolderId: string | null;
  onMove: (folderId: string | null) => Promise<void>;
  trigger?: React.ReactNode;
}) {
  const [isMoving, setIsMoving] = useState(false);

  const handleMove = async (folderId: string | null) => {
    if (folderId === currentFolderId) return;
    setIsMoving(true);
    try {
      await onMove(folderId);
    } finally {
      setIsMoving(false);
    }
  };

  const renderItems = (
    items: ConversationFolder[],
    indent: number = 0,
  ): React.ReactNode[] => {
    const results: React.ReactNode[] = [];

    items.forEach((folder) => {
      const isCurrent = folder.id === currentFolderId;
      results.push(
        <DropdownMenuItem
          key={folder.id}
          onClick={() => handleMove(folder.id)}
          disabled={isCurrent}
          className="pl-2"
          style={{ paddingLeft: `${(indent + 1) * 16 + 8}px` }}
        >
          <span className={isCurrent ? "text-muted-foreground" : ""}>
            {folder.name}
          </span>
          {isCurrent && (
            <span className="ml-auto text-xs text-muted-foreground">(current)</span>
          )}
        </DropdownMenuItem>,
      );
      if (folder.children.length > 0) {
        results.push(...renderItems(folder.children, indent + 1));
      }
    });

    return results;
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        {trigger ?? (
          <Button
            variant="ghost"
            size="sm"
            disabled={isMoving}
            aria-label="Move to folder"
          >
            <FolderInput className="h-4 w-4 mr-1" />
            Move
          </Button>
        )}
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="max-h-72 overflow-y-auto">
        <DropdownMenuItem
          key="__none__"
          onClick={() => handleMove(null)}
          className="pl-2"
        >
          <span>(No folder)</span>
        </DropdownMenuItem>
        {renderItems(folders)}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
