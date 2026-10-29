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
});

describe("errorOf", () => {
  it("is undefined unless the state is an error", () => {
    expect(errorOf({ status: "success", data: 1 })).toBeUndefined();
    expect(errorOf({ status: "loading", data: undefined })).toBeUndefined();
  });
});
