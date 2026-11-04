import { FolderCard } from "@/app/(hermes)/hermes/conversations/components/folder-card";
import { type ConversationFolder } from "@/app/(hermes)/hermes/conversations/components/folder-types";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const folder: ConversationFolder = {
  id: "f-1",
  name: "Recipes",
  isPublic: true,
  parentFolderId: null,
  conversationCount: 1,
  subfolderCount: 0,
  createdAt: "2026-10-01T00:00:00Z",
  updatedAt: "2026-10-01T00:00:00Z",
  children: [],
  depth: 0,
};

describe("FolderCard", () => {
  it("WhenItHasActions_ShouldOfferTheActionsMenu", () => {
    // Arrange & Act
    render(
      <FolderCard
        folder={folder}
        conversationCount={1}
        subfolderCount={0}
        onClick={vi.fn()}
        onCreateSubfolder={vi.fn()}
        onEdit={vi.fn()}
        onDelete={vi.fn()}
      />,
    );

    // Assert
    expect(
      screen.getByRole("button", { name: "Folder actions" }),
    ).toBeInTheDocument();
  });

  it("WhenItHasNoActions_ShouldHideTheActionsMenu", () => {
    // Arrange & Act
    render(
      <FolderCard
        folder={folder}
        conversationCount={1}
        subfolderCount={0}
        onClick={vi.fn()}
      />,
    );

    // Assert
    expect(
      screen.queryByRole("button", { name: "Folder actions" }),
    ).not.toBeInTheDocument();
    expect(screen.getByText("Recipes")).toBeInTheDocument();
  });
});
