import { afterEach, describe, expect, it, vi } from "vitest";
import { ApiError, apiUrl, isNotFound } from "./api-client";

describe("isNotFound", () => {
  it("is true only for a 404 ApiError", () => {
    expect(isNotFound(new ApiError(404, "Not Found"))).toBe(true);
    expect(isNotFound(new ApiError(500, "Server error"))).toBe(false);
    expect(isNotFound(new Error("Network error"))).toBe(false);
  });
});

describe("apiUrl", () => {
  afterEach(() => {
    vi.unstubAllEnvs();
    vi.unstubAllGlobals();
  });

  it("targets the gateway on the host that served the page", () => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE", "");
    vi.stubEnv("NEXT_PUBLIC_API_HOST", "");

    expect(apiUrl("/health")).toBe(
      `${window.location.protocol}//${window.location.hostname}:8300/health`,
    );
  });

  it("prefers a build-time override", () => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE", "https://api.example.test");

    expect(apiUrl("/health")).toBe("https://api.example.test/health");
  });

  it("accepts the legacy NEXT_PUBLIC_API_HOST override", () => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE", "");
    vi.stubEnv("NEXT_PUBLIC_API_HOST", "http://legacy.test:8300");

    expect(apiUrl("/health")).toBe("http://legacy.test:8300/health");
  });

  it("uses API_BASE when rendering on the server", () => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE", "");
    vi.stubEnv("NEXT_PUBLIC_API_HOST", "");
    vi.stubEnv("API_BASE", "http://pantheon-gateway:8300");
    vi.stubGlobal("window", undefined);

    expect(apiUrl("/health")).toBe("http://pantheon-gateway:8300/health");
  });
});
