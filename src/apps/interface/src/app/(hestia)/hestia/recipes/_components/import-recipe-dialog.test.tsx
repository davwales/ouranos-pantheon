import { describe, it, expect, vi, beforeEach } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { ApiError } from "@/lib/api-client";
import { hestiaApi } from "@/lib/api/hestia";
import { ImportRecipeDialog } from "./import-recipe-dialog";

const pushMock = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: pushMock,
  }),
}));

vi.mock("@/lib/api/hestia", () => ({
  hestiaApi: {
    importRecipe: vi.fn(),
  },
}));

vi.mock("@/components/shared/responsive-dialog/responsive-dialog", () => ({
  ResponsiveDialog: ({
    children,
    open,
  }: {
    title: string;
    description: string;
    trigger: React.ReactNode;
    children: React.ReactNode;
    open?: boolean;
    onOpenChange?: (open: boolean) => void;
  }) => (
    <div data-testid="responsive-dialog" data-open={open ? "true" : "false"}>
      {open ? <div data-testid="dialog-content">{children}</div> : null}
    </div>
  ),
}));

function renderDialog(open = true) {
  const onOpenChange = vi.fn();
  render(<ImportRecipeDialog open={open} onOpenChange={onOpenChange} />);
  return { onOpenChange };
}

describe("ImportRecipeDialog", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders the URL input and submit button", () => {
    renderDialog();

    expect(screen.getByLabelText(/recipe url/i)).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /import from link/i }),
    ).toBeInTheDocument();
  });

  it("disables submit and shows a validation message for an invalid URL", () => {
    renderDialog();

    const submit = screen.getByRole("button", {
      name: /import from link/i,
    }) as HTMLButtonElement;
    expect(submit.disabled).toBe(true);

    fireEvent.change(screen.getByLabelText(/recipe url/i), {
      target: { value: "not a url" },
    });

    expect(submit.disabled).toBe(true);
    expect(screen.getByText(/enter a valid http/i)).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/recipe url/i), {
      target: { value: "https://example.com/recipe" },
    });

    expect(submit.disabled).toBe(false);
  });

  it("submits the URL, closes the dialog, and navigates to the recipe", async () => {
    vi.mocked(hestiaApi.importRecipe).mockResolvedValueOnce({
      id: "recipe-123",
    });
    const { onOpenChange } = renderDialog();

    fireEvent.change(screen.getByLabelText(/recipe url/i), {
      target: { value: "https://example.com/recipe" },
    });

    fireEvent.click(screen.getByRole("button", { name: /import from link/i }));

    await waitFor(() => {
      expect(hestiaApi.importRecipe).toHaveBeenCalledWith({
        url: "https://example.com/recipe",
      });
    });

    expect(onOpenChange).toHaveBeenCalledWith(false);
    expect(pushMock).toHaveBeenCalledWith("/hestia/recipes/recipe-123");
  });

  it("renders an error banner and does not navigate on failure", async () => {
    vi.mocked(hestiaApi.importRecipe).mockRejectedValueOnce(
      new ApiError(500, "Internal server error"),
    );
    const { onOpenChange } = renderDialog();

    fireEvent.change(screen.getByLabelText(/recipe url/i), {
      target: { value: "https://example.com/recipe" },
    });

    fireEvent.click(screen.getByRole("button", { name: /import from link/i }));

    await waitFor(() => {
      expect(screen.getByRole("alert")).toHaveTextContent(
        "Internal server error",
      );
    });

    expect(onOpenChange).not.toHaveBeenCalled();
    expect(pushMock).not.toHaveBeenCalled();
  });
});
