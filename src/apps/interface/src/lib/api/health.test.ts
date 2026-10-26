import { afterEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "@/lib/api-client";
import { healthApi, type GetHealthResponseDto } from "./health";

const unhealthy: GetHealthResponseDto = {
  status: "Unhealthy",
  checks: {
    rabbitmq: {
      status: "Unhealthy",
      description: "Connection refused",
      timestamp: "2026-10-23T00:00:00Z",
    },
  },
};

function respond(status: number, body: unknown) {
  vi.stubGlobal(
    "fetch",
    vi.fn().mockResolvedValue(
      new Response(JSON.stringify(body), {
        status,
        headers: { "Content-Type": "application/json" },
      }),
    ),
  );
}

describe("healthApi.getHealthSummary", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("returns the report when the gateway answers 503", async () => {
    respond(503, unhealthy);

    await expect(healthApi.getHealthSummary()).resolves.toEqual(unhealthy);
  });

  it("throws on other error statuses", async () => {
    respond(500, { title: "Internal Server Error" });

    await expect(healthApi.getHealthSummary()).rejects.toBeInstanceOf(
      ApiError,
    );
  });
});
