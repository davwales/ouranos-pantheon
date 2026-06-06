"use client";

import { useCallback, useState } from "react";

export type StreamingChunk = { $type: string } & Record<string, unknown>;

export type ChunkHandlers = {
  onContent?: (content: string) => void;
  onUsage?: (usage: { inputTokens: number; outputTokens: number; totalTokens: number }) => void;
  onSystemPrompt?: (systemPrompt: string) => void;
  onComplete?: () => void;
};

export type StartStreamParams = {
  generator: () => AsyncGenerator<StreamingChunk>;
  chunkHandlers: ChunkHandlers;
  onError?: (error: unknown) => void;
};

export function useStreamingMessage(): {
  isStreaming: boolean;
  streamError: string | null;
  startStream: (params: StartStreamParams) => Promise<void>;
  resetError: () => void;
} {
  const [isStreaming, setIsStreaming] = useState(false);
  const [streamError, setStreamError] = useState<string | null>(null);

  const startStream = useCallback(async (params: StartStreamParams) => {
    setIsStreaming(true);
    setStreamError(null);

    try {
      const gen = params.generator();
      for await (const chunk of gen) {
        switch (chunk.$type) {
          case "content": {
            const content = chunk.content;
            if (typeof content === "string") {
              params.chunkHandlers.onContent?.(content);
            }
            break;
          }
          case "usage": {
            const inputTokens = chunk.inputTokens;
            const outputTokens = chunk.outputTokens;
            const totalTokens = chunk.totalTokens;
            if (
              typeof inputTokens === "number" &&
              typeof outputTokens === "number" &&
              typeof totalTokens === "number"
            ) {
              params.chunkHandlers.onUsage?.({
                inputTokens,
                outputTokens,
                totalTokens,
              });
            }
            break;
          }
          case "systemPrompt": {
            const systemPrompt = chunk.systemPrompt;
            if (typeof systemPrompt === "string") {
              params.chunkHandlers.onSystemPrompt?.(systemPrompt);
            }
            break;
          }
          case "complete": {
            params.chunkHandlers.onComplete?.();
            break;
          }
        }
      }
    } catch (error) {
      params.onError?.(error);
      setStreamError(
        error instanceof Error ? error.message : "An unknown error occurred",
      );
    } finally {
      setIsStreaming(false);
    }
  }, []);

  const resetError = useCallback(() => {
    setStreamError(null);
  }, []);

  return { isStreaming, streamError, startStream, resetError };
}
