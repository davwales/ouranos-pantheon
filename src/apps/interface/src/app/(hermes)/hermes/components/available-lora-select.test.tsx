import { AvailableLoraSelect } from "@/app/(hermes)/hermes/components/available-lora-select";
import { type AvailableLora } from "@/lib/api/hermes";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const detail: AvailableLora = {
  id: "a-1",
  identifier: "detail-xl",
  title: "Detail",
  compatClass: "flux-1",
  defaultWeight: 0.7,
  triggerPhrase: "detailed",
  usageHint: null,
  isCompatible: true,
};

const pixel: AvailableLora = {
  ...detail,
  id: "a-2",
  identifier: "pixel-art",
  title: null,
  isCompatible: false,
};

async function open() {
  fireEvent.click(screen.getByRole("combobox", { name: "LoRA" }));
  return screen.findAllByRole("option");
}

describe("AvailableLoraSelect", () => {
  it("WhenALoraIsChosen_ShouldReportIt", async () => {
    // Arrange
    const onChange = vi.fn();
    render(
      <AvailableLoraSelect
        value=""
        loras={[detail, pixel]}
        takenIdentifiers={new Set()}
        onChange={onChange}
      />,
    );

    // Act
    await open();
    fireEvent.click(screen.getByRole("option", { name: "Detail" }));

    // Assert
    expect(onChange).toHaveBeenCalledWith(detail);
  });

  it("WhenLorasAreTakenOrIncompatible_ShouldLabelThemAndDisableTakenOnes", async () => {
    // Arrange
    render(
      <AvailableLoraSelect
        value=""
        loras={[detail, pixel]}
        takenIdentifiers={new Set(["detail-xl"])}
        onChange={vi.fn()}
      />,
    );

    // Act
    await open();

    // Assert
    expect(
      screen.getByRole("option", { name: "Detail (already added)" }),
    ).toHaveAttribute("aria-disabled", "true");
    expect(
      screen.getByRole("option", { name: "pixel-art (incompatible)" }),
    ).not.toHaveAttribute("aria-disabled", "true");
  });

  it("WhenTheValueIsNoLongerAvailable_ShouldStillShowIt", async () => {
    // Arrange & Act
    render(
      <AvailableLoraSelect
        value="removed"
        loras={[detail]}
        takenIdentifiers={new Set(["removed"])}
        onChange={vi.fn()}
      />,
    );

    // Assert
    expect(screen.getByRole("combobox", { name: "LoRA" })).toHaveTextContent(
      "removed (unavailable)",
    );
  });
});
