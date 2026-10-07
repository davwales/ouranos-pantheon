import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs/use-breadcrumb-label";
import { useBreadcrumbStore } from "@/stores/breadcrumb-store";
import { renderHook } from "@testing-library/react";
import { beforeEach, describe, expect, it } from "vitest";

const SEGMENT = "1b2f3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d";

describe("useBreadcrumbLabel", () => {
  beforeEach(() => {
    useBreadcrumbStore.setState({ labels: {} });
  });

  it("WhenLabelUndefined_ShouldNotRegister", () => {
    // Arrange & Act
    renderHook(() => useBreadcrumbLabel(SEGMENT, undefined));

    // Assert
    expect(useBreadcrumbStore.getState().labels).toEqual({});
  });

  it("WhenLabelDefined_ShouldRegister", () => {
    // Arrange & Act
    renderHook(() => useBreadcrumbLabel(SEGMENT, "Sandbox Market"));

    // Assert
    expect(useBreadcrumbStore.getState().labels[SEGMENT]).toBe("Sandbox Market");
  });

  it("WhenFailedWithoutLabel_ShouldRegisterNotFound", () => {
    // Arrange & Act
    renderHook(() => useBreadcrumbLabel(SEGMENT, undefined, true));

    // Assert
    expect(useBreadcrumbStore.getState().labels[SEGMENT]).toBe("Not found");
  });

  it("WhenFailedWithStaleLabel_ShouldKeepLabel", () => {
    // Arrange & Act
    renderHook(() => useBreadcrumbLabel(SEGMENT, "Sandbox Market", true));

    // Assert
    expect(useBreadcrumbStore.getState().labels[SEGMENT]).toBe("Sandbox Market");
  });

  it("WhenLabelChanges_ShouldUpdateRegistration", () => {
    // Arrange
    const { rerender } = renderHook(({ label }) => useBreadcrumbLabel(SEGMENT, label), {
      initialProps: { label: "Old Name" },
    });

    // Act
    rerender({ label: "New Name" });

    // Assert
    expect(useBreadcrumbStore.getState().labels[SEGMENT]).toBe("New Name");
  });

  it("WhenUnmounted_ShouldClearRegistration", () => {
    // Arrange
    const { unmount } = renderHook(() => useBreadcrumbLabel(SEGMENT, "Sandbox Market"));

    // Act
    unmount();

    // Assert
    expect(useBreadcrumbStore.getState().labels).toEqual({});
  });
});
