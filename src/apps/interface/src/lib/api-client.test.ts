import { afterEach, describe, expect, it, vi } from "vitest";
import { api, ApiError, apiUrl, isNotFound } from "./api-client";

describe("api errors", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("uses the problem detail so the reason reaches the user", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({ status: 409, title: "Conflict", detail: "The persona 'Sage' is in use." }),
          { status: 409 },
        ),
      ),
    );

    await expect(api.del("/api/hermes/personas/1")).rejects.toMatchObject({
      status: 409,
      message: "The persona 'Sage' is in use.",
    });
  });

  it("falls back to the problem title without a detail", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ status: 400, title: "Bad request" }), { status: 400 }),
      ),
    );

    await expect(api.get("/api/hermes/personas")).rejects.toMatchObject({
      status: 400,
      message: "Bad request",
    });
  });
});

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
