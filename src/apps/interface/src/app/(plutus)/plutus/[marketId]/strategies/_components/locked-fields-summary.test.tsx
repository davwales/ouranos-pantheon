import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { DEFAULT_FLIP_CONFIGURATION } from "./flip-constants";
import { LockedFieldsSummary } from "./locked-fields-summary";

describe("LockedFieldsSummary", () => {
  it("WhenNothingLocked_ShouldSayEveryFieldIsSearched", () => {
    // Arrange & Act
    render(<LockedFieldsSummary configuration={DEFAULT_FLIP_CONFIGURATION} />);

    // Assert
    expect(screen.getByText(/Nothing locked/)).toBeInTheDocument();
  });

  it("WhenFieldsLocked_ShouldListThemWithTheirValues", () => {
    // Arrange & Act
    render(
      <LockedFieldsSummary
        configuration={{
          ...DEFAULT_FLIP_CONFIGURATION,
          lockedParameters: ["maxHoldHours"],
        }}
      />,
    );

    // Assert
    expect(screen.getByText(/Max Hold Hours 72/)).toBeInTheDocument();
  });
});
