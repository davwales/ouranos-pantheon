import type { FolderSummary } from "@/lib/api/hermes-types";
import type { ConversationFolder } from "../components/folder-types";

export function buildFolderTree(
  folders: FolderSummary[],
  parentId: string | null = null,
  depth: number = 0,
): ConversationFolder[] {
  const roots = folders.filter((f) => f.parentFolderId === parentId);
  return roots.map((folder) => ({
    ...folder,
    children: buildFolderTree(folders, folder.id, depth + 1),
    depth,
  }));
}

export function flattenFolders(
  folders: ConversationFolder[],
): ConversationFolder[] {
  const result: ConversationFolder[] = [];
  for (const folder of folders) {
    result.push(folder);
    result.push(...flattenFolders(folder.children));
  }
  return result;
}
