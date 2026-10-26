import { render, screen } from "@testing-library/react";
import { Zap } from "lucide-react";
import { describe, expect, it } from "vitest";
import { type ApiState } from "@/hooks/use-api";
import { DashboardCard } from "./dashboard-card";

function renderCard(state: ApiState<string[]>, href?: string) {
  return render(
    <DashboardCard
      title="Things"
      icon={Zap}
      href={href}
      state={state}
      isEmpty={(data) => data.length === 0}
      emptyText="Nothing here."
    >
      {(data) => <p>{data.join(", ")}</p>}
    </DashboardCard>,
  );
}

describe("DashboardCard", () => {
  it("renders the title and a view-all link", () => {
    renderCard({ status: "success", data: ["a"] }, "/things");

    expect(screen.getByText("Things")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "View all" })).toHaveAttribute(
      "href",
      "/things",
    );
  });

  it("renders the content when data is present", () => {
    renderCard({ status: "success", data: ["a", "b"] });

    expect(screen.getByText("a, b")).toBeInTheDocument();
  });

  it("renders the empty text when data is empty", () => {
    renderCard({ status: "success", data: [] });

    expect(screen.getByText("Nothing here.")).toBeInTheDocument();
  });

  it("renders an error when loading failed without data", () => {
    renderCard({ status: "error", data: undefined, error: new Error("x") });

    expect(screen.getByText(/couldn’t load/i)).toBeInTheDocument();
  });

  it("keeps showing stale data while reloading", () => {
    renderCard({ status: "loading", data: ["a"] });

    expect(screen.getByText("a")).toBeInTheDocument();
  });
});
