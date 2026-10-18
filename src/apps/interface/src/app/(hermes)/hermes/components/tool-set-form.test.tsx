import {
  ToolSetForm,
  toSaveToolSetInput,
} from "@/app/(hermes)/hermes/components/tool-set-form";
import { type ToolSetFormInput } from "@/app/(hermes)/hermes/types";
import { type AvailableTool } from "@/lib/api/hermes";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const generateImage: AvailableTool = {
  source: "Native",
  name: "generate_image",
  description: "Draw the current scene.",
};

const roleplay: ToolSetFormInput = {
  name: "Roleplay",
  description: "",
  instructions: "",
  isPublic: true,
  tools: [
    {
      source: "Native",
      name: "generate_image",
      additionalDescription: "Use it freely.",
    },
  ],
};

function save() {
  fireEvent.click(screen.getByRole("button", { name: "Save" }));
}

describe("ToolSetForm", () => {
  it("WhenAToolIsEnabledWithGuidance_ShouldSaveItWithTheAdditionalDescription", () => {
    // Arrange
    const onSave = vi.fn();
    render(<ToolSetForm availableTools={[generateImage]} onSave={onSave} />);
    fireEvent.change(screen.getByLabelText("Name"), {
      target: { value: "Default" },
    });

    // Act
    fireEvent.click(screen.getByLabelText("generate_image"));
    fireEvent.change(
      screen.getByLabelText("Additional description for generate_image"),
      { target: { value: "Only when asked." } },
    );
    save();

    // Assert
    expect(onSave).toHaveBeenCalledWith(
      expect.objectContaining({
        name: "Default",
        tools: [
          {
            source: "Native",
            name: "generate_image",
            additionalDescription: "Only when asked.",
          },
        ],
      }),
    );
  });

  it("WhenAToolIsDisabled_ShouldSaveWithoutIt", () => {
    // Arrange
    const onSave = vi.fn();
    render(
      <ToolSetForm
        availableTools={[generateImage]}
        initial={roleplay}
        onSave={onSave}
      />,
    );

    // Act
    fireEvent.click(screen.getByLabelText("generate_image"));
    save();

    // Assert
    expect(onSave).toHaveBeenCalledWith(
      expect.objectContaining({ tools: [] }),
    );
  });

  it("WhenAToolIsReEnabled_ShouldKeepItsDraftDescription", () => {
    // Arrange
    const onSave = vi.fn();
    render(
      <ToolSetForm
        availableTools={[generateImage]}
        initial={roleplay}
        onSave={onSave}
      />,
    );

    // Act
    fireEvent.click(screen.getByLabelText("generate_image"));
    fireEvent.click(screen.getByLabelText("generate_image"));
    save();

    // Assert
    expect(onSave).toHaveBeenCalledWith(
      expect.objectContaining({ tools: roleplay.tools }),
    );
  });

  it("WhenASavedToolIsNoLongerOffered_ShouldShowItAsUnavailable", () => {
    // Act
    render(
      <ToolSetForm availableTools={[]} initial={roleplay} onSave={vi.fn()} />,
    );

    // Assert
    expect(screen.getByText("Unavailable")).toBeDefined();
    expect(
      (screen.getByLabelText("generate_image") as HTMLInputElement).checked,
    ).toBe(true);
  });

  it("WhenNameIsBlank_ShouldDisableSave", () => {
    // Act
    render(<ToolSetForm availableTools={[generateImage]} onSave={vi.fn()} />);

    // Assert
    expect(
      (screen.getByRole("button", { name: "Save" }) as HTMLButtonElement)
        .disabled,
    ).toBe(true);
  });
});

describe("toSaveToolSetInput", () => {
  it("WhenOptionalTextIsBlank_ShouldSendNull", () => {
    // Arrange
    const input: ToolSetFormInput = {
      name: "  Default  ",
      description: " ",
      instructions: "",
      isPublic: false,
      tools: [
        { source: "Native", name: "generate_image", additionalDescription: " " },
      ],
    };

    // Act
    const result = toSaveToolSetInput(input);

    // Assert
    expect(result).toEqual({
      name: "Default",
      description: null,
      instructions: null,
      isPublic: false,
      tools: [
        { source: "Native", name: "generate_image", additionalDescription: null },
      ],
    });
  });
});
