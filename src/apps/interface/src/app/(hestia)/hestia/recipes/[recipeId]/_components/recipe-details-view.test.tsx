import { describe, expect, it } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import type { RecipeDetails } from "@/lib/api/hestia-types";
import { RecipeDetailsView } from "./recipe-details-view";

const details: RecipeDetails = {
  yield: "8 slices",
  prepMinutes: 15,
  cookMinutes: 75,
  totalMinutes: null,
  imageUrl: "https://example.com/cake.jpg",
};

describe("RecipeDetailsView", () => {
  it("renders yield, the times that are set, and the image", () => {
    const { container } = render(<RecipeDetailsView details={details} />);

    expect(screen.getByText("Yield")).toBeInTheDocument();
    expect(screen.getByText("8 slices")).toBeInTheDocument();
    expect(screen.getByText("15 min")).toBeInTheDocument();
    expect(screen.getByText("1 hr 15 min")).toBeInTheDocument();
    expect(screen.queryByText("Total")).not.toBeInTheDocument();
    expect(container.querySelector("img")).toHaveAttribute("src", details.imageUrl);
  });

  it("renders nothing when there are no details", () => {
    const { container } = render(<RecipeDetailsView details={null} />);

    expect(container).toBeEmptyDOMElement();
  });

  it("hides the image when it fails to load", () => {
    const { container } = render(
      <RecipeDetailsView details={{ ...details, yield: null, prepMinutes: null, cookMinutes: null }} />,
    );

    fireEvent.error(container.querySelector("img")!);

    expect(container).toBeEmptyDOMElement();
  });
});
