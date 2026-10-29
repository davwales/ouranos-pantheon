import { LoraForm } from "@/app/(hermes)/hermes/components/lora-form";
import { type AvailableLora } from "@/lib/api/hermes";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const detail: AvailableLora = {
  id: "a-1",
  identifier: "detail-xl",
  title: "Detail",
  compatClass: "sdxl-1",
  defaultWeight: 0.7,
  triggerPhrase: "detailed",
  usageHint: "Works best at 0.7.",
  isCompatible: false,
};

vi.mock("@/lib/api/hermes", () => ({
  hermesApi: {
    getAvailableLoras: () => Promise.resolve([detail]),
    getAllLoras: () => Promise.resolve([]),
  },
}));

describe("LoraForm", () => {
  it("WhenALoraIsPicked_ShouldPrefillItsDetailsAndSaveThem", async () => {
    // Arrange
    const onSave = vi.fn();
    render(<LoraForm onSave={onSave} />);

    // Act
    fireEvent.click(screen.getByRole("combobox", { name: "LoRA" }));
    fireEvent.click(await screen.findByRole("option", { name: /Detail/ }));
    fireEvent.click(screen.getByRole("button", { name: "Save" }));

    // Assert
    expect(onSave).toHaveBeenCalledWith({
      name: "Detail",
      identifier: "detail-xl",
      weight: 0.7,
      isPublic: true,
      triggerPhrase: "detailed",
    });
  });

  it("WhenTheLoraIsIncompatible_ShouldWarnAndShowItsUsageHint", async () => {
    // Arrange
    render(<LoraForm onSave={vi.fn()} />);

    // Act
    fireEvent.click(screen.getByRole("combobox", { name: "LoRA" }));
    fireEvent.click(await screen.findByRole("option", { name: /Detail/ }));

    // Assert
    expect(screen.getByText(/Made for sdxl-1 models/)).toBeInTheDocument();
    expect(screen.getByText("Works best at 0.7.")).toBeInTheDocument();
  });

  it("WhenNoLoraIsPicked_ShouldNotAllowSaving", () => {
    // Arrange & Act
    render(<LoraForm onSave={vi.fn()} />);

    // Assert
    expect(screen.getByRole("button", { name: "Save" })).toBeDisabled();
  });
});
