import { act, renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { errorOf, useApi } from "./use-api";

describe("useApi", () => {
  it("keeps the last data when a refetch fails", async () => {
    // Arrange
    const error = new Error("Gateway timeout");
    const fetcher = vi
      .fn<() => Promise<string>>()
      .mockResolvedValueOnce("first")
      .mockRejectedValueOnce(error);
    const { result } = renderHook(() => useApi(fetcher));
    await waitFor(() => expect(result.current[0].status).toBe("success"));

    // Act
    act(() => result.current[1]());
    await waitFor(() => expect(result.current[0].status).toBe("error"));

    // Assert
    expect(result.current[0].data).toBe("first");
    expect(errorOf(result.current[0])).toBe(error);
  });

  it("keeps the previous data reference when a refetch returns equal data", async () => {
    // Arrange
    const first = { trades: [{ price: 1 }] };
    const fetcher = vi
      .fn<() => Promise<typeof first>>()
      .mockResolvedValueOnce(first)
      .mockResolvedValueOnce({ trades: [{ price: 1 }] });
    const { result } = renderHook(() => useApi(fetcher));
    await waitFor(() => expect(result.current[0].status).toBe("success"));

    // Act
    act(() => result.current[1]());
    await waitFor(() => expect(result.current[0].status).toBe("success"));

    // Assert
    expect(fetcher).toHaveBeenCalledTimes(2);
    expect(result.current[0].data).toBe(first);
  });

  it("replaces the data when a refetch returns changed data", async () => {
    // Arrange
    const second = { trades: [{ price: 2 }] };
    const fetcher = vi
      .fn<() => Promise<typeof second>>()
      .mockResolvedValueOnce({ trades: [{ price: 1 }] })
      .mockResolvedValueOnce(second);
    const { result } = renderHook(() => useApi(fetcher));
    await waitFor(() => expect(result.current[0].status).toBe("success"));

    // Act
    act(() => result.current[1]());
    await waitFor(() => expect(result.current[0].data).toBe(second));

    // Assert
    expect(result.current[0].status).toBe("success");
  });
});

describe("errorOf", () => {
  it("is undefined unless the state is an error", () => {
    expect(errorOf({ status: "success", data: 1 })).toBeUndefined();
    expect(errorOf({ status: "loading", data: undefined })).toBeUndefined();
  });
});
