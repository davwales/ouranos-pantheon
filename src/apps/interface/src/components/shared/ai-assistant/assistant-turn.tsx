import { ActionBlock } from "@/components/shared/ai-assistant/blocks/action-block";
import { ActionGroupBar } from "@/components/shared/ai-assistant/blocks/action-group-bar";
import { ImageBlock } from "@/components/shared/ai-assistant/blocks/image-block";
import { PendingBlock } from "@/components/shared/ai-assistant/blocks/pending-block";
import { ReasoningBlock } from "@/components/shared/ai-assistant/blocks/reasoning-block";
import { TextBlock } from "@/components/shared/ai-assistant/blocks/text-block";
import {
  ActivityGroup,
  type ActivityPart,
} from "@/components/shared/ai-assistant/blocks/activity-group";
import {
  type AssistantChatPart,
  type AssistantChatToolPart,
} from "@/components/shared/ai-assistant/assistant-chat-parts";
import { type AssistantActionHandlers } from "@/lib/api/assistant";
import { cn } from "@/lib/utils";

type Block =
  | Exclude<AssistantChatPart, AssistantChatToolPart>
  | { kind: "activity"; id: string; parts: ActivityPart[] };

export type AssistantTurnProps = React.ComponentProps<"div"> & {
  name: string;
  parts: AssistantChatPart[];
  context?: unknown;
  isStreaming?: boolean;
  actionHandlers?: AssistantActionHandlers;
  onApplyAction?: (actionId: string) => void;
  onRejectAction?: (actionId: string) => void;
  onApplyAll?: () => void;
  onRejectAll?: () => void;
};

export function AssistantTurn({
  name,
  parts,
  context,
  isStreaming = false,
  actionHandlers,
  onApplyAction,
  onRejectAction,
  onApplyAll,
  onRejectAll,
  className,
  ...props
}: AssistantTurnProps) {
  const blocks = toBlocks(parts);
  const last = parts.at(-1);
  const streamingId =
    isStreaming && last?.kind === "reasoning" && last.endedAt === undefined ? last.id : undefined;
  const proposed = parts.filter(
    (part) => part.kind === "action" && part.status === "proposed" && actionHandlers?.[part.name],
  );
  const isApplying = parts.some((part) => part.kind === "action" && part.status === "applying");

  return (
    <div className={cn("flex w-full min-w-0 flex-col gap-2.5", className)} {...props}>
      <span className="text-xs font-medium text-muted-foreground">{name}</span>
      {blocks.map((block) => {
        switch (block.kind) {
          case "reasoning":
            return (
              <ReasoningBlock
                key={block.id}
                content={block.content}
                isStreaming={block.id === streamingId}
                startedAt={block.startedAt}
                endedAt={block.endedAt}
              />
            );
          case "text":
            return <TextBlock key={block.id} content={block.content} />;
          case "activity":
            return (
              <ActivityGroup
                key={block.id}
                parts={block.parts}
                isActive={isStreaming && block === blocks.at(-1)}
                streamingId={streamingId}
                context={context}
              />
            );
          case "image":
            return <ImageBlock key={block.id} image={block} />;
          case "action":
            return (
              <ActionBlock
                key={block.id}
                action={block}
                handler={actionHandlers?.[block.name]}
                context={context}
                onApply={() => onApplyAction?.(block.id)}
                onReject={() => onRejectAction?.(block.id)}
              />
            );
        }
      })}
      {isStreaming && isAwaitingModel(last) && <PendingBlock />}
      {!isStreaming && proposed.length > 1 && onApplyAll && onRejectAll && (
        <ActionGroupBar
          count={proposed.length}
          disabled={isApplying}
          onApplyAll={onApplyAll}
          onRejectAll={onRejectAll}
        />
      )}
    </div>
  );
}

// Between a tool's result and the model's next words nothing streams, so the turn shows that
// the model is still working.
function isAwaitingModel(last: AssistantChatPart | undefined): boolean {
  if (last === undefined) {
    return true;
  }
  if (last.kind === "tool") {
    return last.status !== "running";
  }
  if (last.kind === "image") {
    return true;
  }
  return last.kind === "action" && last.output !== undefined;
}

// Reasoning on its own stays a light inline row; once a run of reasoning includes a tool call,
// the whole run becomes one activity stack.
function toBlocks(parts: AssistantChatPart[]): Block[] {
  const blocks: Block[] = [];
  let run: ActivityPart[] = [];

  const flush = () => {
    if (run.some((part) => part.kind === "tool")) {
      blocks.push({ kind: "activity", id: run[0].id, parts: run });
    } else {
      blocks.push(...run.filter((part) => part.kind === "reasoning"));
    }
    run = [];
  };

  for (const part of parts) {
    if (part.kind === "reasoning" || part.kind === "tool") {
      run.push(part);
    } else {
      flush();
      blocks.push(part);
    }
  }
  flush();
  return blocks;
}
