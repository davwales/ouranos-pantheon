import { ActionCard } from "@/components/shared/ai-assistant/action-card";
import { toolLabel } from "@/components/shared/ai-assistant/tool-labels";
import { type AssistantChatAction } from "@/components/shared/ai-assistant/use-assistant-chat";
import { Typography } from "@/components/shared/typography";
import { LoadingSegment } from "@/components/shared/ai-assistant/segments/loading-segment";
import { TextSegment } from "@/components/shared/ai-assistant/segments/text-segment";
import { ThinkingSegment } from "@/components/shared/ai-assistant/segments/thinking-segment";
import { ToolSegment } from "@/components/shared/ai-assistant/segments/tool-segment";
import {
  parseSegments,
  type Segment,
} from "@/components/shared/ai-assistant/parse-segments";
import {
  type AssistantActionHandlers,
  type AssistantRole,
} from "@/lib/api/assistant";
import { cn } from "@/lib/utils";

export function Message({
  name,
  role,
  content,
  reasoning = "",
  isStreaming = false,
  tools = [],
  actions = [],
  toolLabels,
  actionHandlers,
  onApplyAction,
  ...props
}: React.ComponentProps<"div"> & {
  name: string;
  role: AssistantRole;
  content: string;
  reasoning?: string;
  isStreaming?: boolean;
  tools?: string[];
  actions?: AssistantChatAction[];
  toolLabels?: Record<string, string>;
  actionHandlers?: AssistantActionHandlers;
  onApplyAction?: (actionId: string) => void;
}) {
  const isUser = role === "User";
  const segments = isUser
    ? [{ type: "text" as const, content }]
    : assistantSegments(content, reasoning, isStreaming);
  const labels = [...new Set(tools.map((tool) => toolLabel(tool, toolLabels)))];

  return (
    <div {...props}>
      <div className={cn("py-2 px-4 border rounded-2xl", isUser && "bg-accent/30")}>
        {labels.length > 0 && <ToolSegment labels={labels} />}
        {segments.map((segment, i) => {
          switch (segment.type) {
            case "loading":
              return <LoadingSegment key={`loading-${i}`} />;
            case "thinking":
              return (
                <ThinkingSegment
                  key={`thinking-${i}`}
                  content={segment.content}
                  isStreaming={segment.isStreaming}
                />
              );
            case "text":
              return (
                <TextSegment key={`text-${i}`} content={segment.content} />
              );
          }
        })}
        {actions.map((action) => {
          const handler = actionHandlers?.[action.name];
          return handler ? (
            <ActionCard
              key={action.id}
              description={handler.describe(action.action)}
              action={action}
              onApply={() => onApplyAction?.(action.id)}
            />
          ) : null;
        })}
      </div>
      <Typography
        variant="muted"
        className={cn("mx-2.5 my-1", isUser && "text-right")}
      >
        {name}
      </Typography>
    </div>
  );
}

function assistantSegments(
  content: string,
  reasoning: string,
  isStreaming: boolean,
): Segment[] {
  if (reasoning.length === 0) {
    return parseSegments(content, isStreaming);
  }

  const isThinking = isStreaming && content.length === 0;
  return [
    { type: "thinking", content: reasoning, isStreaming: isThinking },
    ...(isThinking ? [] : parseSegments(content, isStreaming)),
  ];
}
