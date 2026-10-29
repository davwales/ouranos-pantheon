import { ImageLorasPanel } from "@/app/(hermes)/hermes/components/image-loras-panel";
import { type Lora } from "@/lib/api/hermes";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const detail: Lora = {
  id: "lora-1",
  name: "Detail",
  identifier: "add-detail",
  weight: 0.8,
  isPublic: true,
  triggerPhrase: null,
  isUnavailable: false,
  isIncompatible: false,
};

const pixel: Lora = {
  id: "lora-2",
  name: "Pixel Art",
  identifier: "pixel-art",
  weight: 1,
  isPublic: true,
  triggerPhrase: "pixel art",
  isUnavailable: false,
  isIncompatible: true,
};

const removed: Lora = {
  id: "lora-3",
  name: "Removed",
  identifier: "removed",
  weight: 1,
  isPublic: true,
  triggerPhrase: null,
  isUnavailable: true,
  isIncompatible: false,
};

vi.mock("@/lib/api/hermes", () => ({
  hermesApi: { getAllLoras: () => Promise.resolve([detail, pixel, removed]) },
}));

async function openPanel() {
  fireEvent.click(screen.getByRole("button", { name: /Image LoRAs/ }));
  return screen.findByText("Pixel Art");
}

describe("ImageLorasPanel", () => {
  it("WhenAnInactiveLoraIsClicked_ShouldAddIt", async () => {
    // Arrange
    const onLorasChange = vi.fn();
    render(<ImageLorasPanel activeLoras={[detail]} onLorasChange={onLorasChange} />);

    // Act
    fireEvent.click(await openPanel());

    // Assert
    expect(onLorasChange).toHaveBeenCalledWith([detail, pixel]);
  });

  it("WhenAnActiveLoraIsClicked_ShouldRemoveIt", async () => {
    // Arrange
    const onLorasChange = vi.fn();
    render(<ImageLorasPanel activeLoras={[detail, pixel]} onLorasChange={onLorasChange} />);

    // Act
    fireEvent.click(await openPanel());

    // Assert
    expect(onLorasChange).toHaveBeenCalledWith([detail]);
  });

  it("WhenLorasAreActive_ShouldShowTheActiveCount", () => {
    // Arrange & Act
    render(<ImageLorasPanel activeLoras={[detail]} />);

    // Assert
    expect(screen.getByText("1 active")).toBeInTheDocument();
  });

  it("WhenAnUnavailableLoraIsInactive_ShouldNotAddIt", async () => {
    // Arrange
    const onLorasChange = vi.fn();
    render(<ImageLorasPanel activeLoras={[]} onLorasChange={onLorasChange} />);
    await openPanel();

    // Act
    fireEvent.click(screen.getByText("Removed"));

    // Assert
    expect(onLorasChange).not.toHaveBeenCalled();
    expect(screen.getByText("Unavailable")).toBeInTheDocument();
  });

  it("WhenAnUnavailableLoraIsActive_ShouldStillRemoveIt", async () => {
    // Arrange
    const onLorasChange = vi.fn();
    render(<ImageLorasPanel activeLoras={[removed]} onLorasChange={onLorasChange} />);
    await openPanel();

    // Act
    fireEvent.click(screen.getByText("Removed"));

    // Assert
    expect(onLorasChange).toHaveBeenCalledWith([]);
  });

  it("WhenALoraIsIncompatible_ShouldWarnAboutIt", async () => {
    // Arrange & Act
    render(<ImageLorasPanel activeLoras={[]} />);
    await openPanel();

    // Assert
    expect(screen.getByText("Incompatible")).toBeInTheDocument();
  });
});
