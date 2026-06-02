import type { FolderSummary } from "@/lib/api/hermes-types";

export type ConversationFolder = FolderSummary & {
  children: ConversationFolder[];
  depth: number;
};
