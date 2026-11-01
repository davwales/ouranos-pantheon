import { toChatMessage, toMessageInput } from "@/hooks/use-message-controller";
import { Role } from "@/lib/api/hermes-types";
import { describe, expect, it } from "vitest";

describe("toChatMessage", () => {
  it("ShouldGiveEachMessageItsOwnClientId", () => {
    // Arrange
    const message = { role: Role.User, content: "Hi" };

    // Act
    const first = toChatMessage(message);
    const second = toChatMessage(message);

    // Assert
    expect(first).toMatchObject(message);
    expect(first.clientId).toBeTruthy();
    expect(first.clientId).not.toBe(second.clientId);
  });
});

describe("toMessageInput", () => {
  it("ShouldDropTheClientIdBeforeTheMessageIsSent", () => {
    // Arrange
    const message = { clientId: "client-1", role: Role.Assistant, content: "Hello", sortOrder: 2 };

    // Act
    const input = toMessageInput(message);

    // Assert
    expect(input).toEqual({ role: Role.Assistant, content: "Hello", sortOrder: 2 });
  });
});
