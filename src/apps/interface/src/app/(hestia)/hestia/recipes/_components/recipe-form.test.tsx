import { describe, it, expect, vi, beforeEach } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { RecipeForm } from "./recipe-form";
import { UNIT_OPTIONS } from "./unit-options";
import { hestiaApi } from "@/lib/api/hestia";
import { ApiError } from "@/lib/api-client";

const pushMock = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: pushMock,
  }),
}));

vi.mock("@/lib/api/hestia", () => ({
  hestiaApi: {
    createRecipe: vi.fn(),
  },
}));

async function fillIngredientRow(
  quantity: string,
  unit: string,
  name: string,
  rowIndex = 0,
) {
  const suffix = rowIndex + 1;
  fireEvent.change(
    screen.getByLabelText(new RegExp(`quantity for ingredient ${suffix}`, "i")),
    { target: { value: quantity } },
  );

  const unitTrigger = screen.getByRole("combobox", {
    name: new RegExp(`unit for ingredient ${suffix}`, "i"),
  });
  fireEvent.click(unitTrigger);

  const normalizedUnit = unit.toLowerCase();
  if (UNIT_OPTIONS.includes(normalizedUnit as (typeof UNIT_OPTIONS)[number])) {
    const option = await screen.findByRole("option", {
      name: new RegExp(`^${normalizedUnit}$`, "i"),
    });
    fireEvent.click(option);
  } else {
    const otherOption = await screen.findByRole("option", {
      name: /^other…$/i,
    });
    fireEvent.click(otherOption);

    fireEvent.change(
      screen.getByRole("textbox", {
        name: new RegExp(`unit for ingredient ${suffix}`, "i"),
      }),
      { target: { value: unit } },
    );
  }

  fireEvent.change(
    screen.getByLabelText(new RegExp(`name for ingredient ${suffix}`, "i")),
    { target: { value: name } },
  );
}

function addIngredientRow() {
  fireEvent.click(screen.getByRole("button", { name: /add ingredient/i }));
}

describe("RecipeForm", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders all labels, inputs, and Save button", () => {
    render(<RecipeForm />);

    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /more options/i })).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: /more options/i }));
    expect(screen.getByLabelText(/source url/i)).toBeInTheDocument();
    expect(
      screen.getByLabelText(/quantity for ingredient 1/i),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("combobox", { name: /unit for ingredient 1/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText(/name for ingredient 1/i),
    ).toBeInTheDocument();
    expect(screen.getByLabelText(/text for step 1/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/notes/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /save/i })).toBeInTheDocument();
  });

  it("typing into Title updates its value", () => {
    render(<RecipeForm />);

    const titleInput = screen.getByLabelText(/title/i);
    fireEvent.change(titleInput, { target: { value: "Spaghetti Carbonara" } });

    expect(titleInput).toHaveValue("Spaghetti Carbonara");
  });

  it("shows inline errors and does not submit when required fields are empty", async () => {
    render(<RecipeForm />);

    const saveButton = screen.getByRole("button", { name: /save/i }) as HTMLButtonElement;
    fireEvent.submit(saveButton.form!);

    await waitFor(() => {
      expect(screen.getAllByRole("alert").length).toBeGreaterThan(0);
    });

    expect(hestiaApi.createRecipe).not.toHaveBeenCalled();
  });

  it("shows 'Unit is required' when 'Other…' is picked but left empty", async () => {
    render(<RecipeForm />);

    fireEvent.change(screen.getByLabelText(/title/i), {
      target: { value: "Test Recipe" },
    });

    const unitTrigger = screen.getByRole("combobox", {
      name: /unit for ingredient 1/i,
    });
    fireEvent.click(unitTrigger);

    const otherOption = await screen.findByRole("option", { name: /^other…$/i });
    fireEvent.click(otherOption);

    fireEvent.change(screen.getByLabelText(/name for ingredient 1/i), {
      target: { value: "flour" },
    });
    fireEvent.change(screen.getByLabelText(/quantity for ingredient 1/i), {
      target: { value: "2" },
    });
    fireEvent.change(screen.getByLabelText(/text for step 1/i), {
      target: { value: "Mix and bake." },
    });

    fireEvent.click(screen.getByRole("button", { name: /save/i }));

    await waitFor(() => {
      expect(screen.getByText("Unit is required")).toBeInTheDocument();
    });

    expect(hestiaApi.createRecipe).not.toHaveBeenCalled();
  });

  it("submits the form and redirects on success", async () => {
    vi.mocked(hestiaApi.createRecipe).mockResolvedValueOnce({ id: "recipe-123" });

    render(<RecipeForm />);

    fireEvent.change(screen.getByLabelText(/title/i), {
      target: { value: "Spaghetti Carbonara" },
    });

    await fillIngredientRow("400", "g", "spaghetti");
    addIngredientRow();
    await fillIngredientRow("4", "large", "eggs", 1);

    fireEvent.change(screen.getByLabelText(/text for step 1/i), {
      target: { value: "Boil pasta. Mix eggs." },
    });

    fireEvent.click(screen.getByRole("button", { name: /save/i }));

    await waitFor(() => {
      expect(hestiaApi.createRecipe).toHaveBeenCalledWith({
        title: "Spaghetti Carbonara",
        sourceUrl: null,
        ingredients: [
          { quantity: 400, unit: "g", name: "spaghetti" },
          { quantity: 4, unit: "large", name: "eggs" },
        ],
        steps: [{ text: "Boil pasta. Mix eggs." }],
        notes: "",
      });
    });

    expect(pushMock).toHaveBeenCalledWith("/hestia/recipes");
  });

  it("renders a generic error banner for a 400 response without redirecting", async () => {
    vi.mocked(hestiaApi.createRecipe).mockRejectedValueOnce(
      new ApiError(400, "Required input Title was empty."),
    );

    render(<RecipeForm />);

    fireEvent.change(screen.getByLabelText(/title/i), {
      target: { value: "Existing Recipe" },
    });

    await fillIngredientRow("2", "cups", "flour");
    addIngredientRow();
    await fillIngredientRow("1", "cup", "water", 1);

    fireEvent.change(screen.getByLabelText(/text for step 1/i), {
      target: { value: "Mix and bake." },
    });

    fireEvent.click(screen.getByRole("button", { name: /save/i }));

    await waitFor(() => {
      expect(screen.getByRole("alert")).toHaveTextContent(
        "Required input Title was empty.",
      );
    });

    expect(
      screen.queryByText(/Title is already in use/i),
    ).not.toBeInTheDocument();
    expect(pushMock).not.toHaveBeenCalled();
  });

  it("renders a generic error banner for a 500 response without redirecting", async () => {
    vi.mocked(hestiaApi.createRecipe).mockRejectedValueOnce(
      new ApiError(500, "Internal server error"),
    );

    render(<RecipeForm />);

    fireEvent.change(screen.getByLabelText(/title/i), {
      target: { value: "Spaghetti Carbonara" },
    });

    await fillIngredientRow("400", "g", "spaghetti");

    fireEvent.change(screen.getByLabelText(/text for step 1/i), {
      target: { value: "Boil pasta." },
    });

    fireEvent.click(screen.getByRole("button", { name: /save/i }));

    await waitFor(() => {
      expect(screen.getByRole("alert")).toHaveTextContent(
        "Internal server error",
      );
    });

    expect(pushMock).not.toHaveBeenCalled();
  });
});
