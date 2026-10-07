import { useBreadcrumbStore } from "@/stores/breadcrumb-store";
import { beforeEach, describe, expect, it } from "vitest";

describe("useBreadcrumbStore", () => {
  beforeEach(() => {
    useBreadcrumbStore.setState({ labels: {} });
  });

  it("WhenLabelSet_ShouldStoreLabelForSegment", () => {
    // Arrange & Act
    useBreadcrumbStore.getState().setLabel("abc", "Sandbox Market");

    // Assert
    expect(useBreadcrumbStore.getState().labels).toEqual({ abc: "Sandbox Market" });
  });

  it("WhenLabelCleared_ShouldRemoveOnlyThatSegment", () => {
    // Arrange
    useBreadcrumbStore.setState({ labels: { abc: "Market", def: "Strategy" } });

    // Act
    useBreadcrumbStore.getState().clearLabel("abc");

    // Assert
    expect(useBreadcrumbStore.getState().labels).toEqual({ def: "Strategy" });
  });
});
