"use client";

import { Button } from "@/components/ui/button";
import { MessageSquarePlus } from "lucide-react";
import Link from "next/link";
import { type ReactNode } from "react";

import { FolderBreadcrumb } from "./folder-breadcrumb";
import type { ConversationFolder } from "./folder-types";

export function ConversationToolbar({
  currentFolderId,
  allFolders,
  onNavigate,
  children,
}: {
  currentFolderId: string | null;
  allFolders: ConversationFolder[];
  onNavigate: (folderId: string | null) => void;
  children?: ReactNode;
}) {
  return (
    <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
      <FolderBreadcrumb
        currentFolderId={currentFolderId}
        folders={allFolders}
        onNavigate={onNavigate}
      />
      <div className="flex items-center gap-2">
        {children}
        <Button asChild variant="default" size="sm">
          <Link href="/hermes/chat">
            <MessageSquarePlus className="h-4 w-4" />
            New Chat
          </Link>
        </Button>
      </div>
    </div>
  );
}
