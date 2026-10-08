import { Typography } from "@/components/shared/typography";
import { LoadingSegment } from "@/components/shared/ai-assistant/segments/loading-segment";
import { TextSegment } from "@/components/shared/ai-assistant/segments/text-segment";
import { ThinkingSegment } from "@/components/shared/ai-assistant/segments/thinking-segment";
import { parseSegments } from "@/components/shared/ai-assistant/parse-segments";
import { type AssistantRole } from "@/lib/api/assistant";
import { cn } from "@/lib/utils";

export function Message({
  name,
  role,
  content,
  isStreaming = false,
  ...props
}: React.ComponentProps<"div"> & {
  name: string;
  role: AssistantRole;
  content: string;
  isStreaming?: boolean;
}) {
  const isUser = role === "User";
  const segments = isUser
    ? [{ type: "text" as const, content }]
    : parseSegments(content, isStreaming);

  return (
    <div {...props}>
      <div className={cn("py-2 px-4 border rounded-2xl", isUser && "bg-accent/30")}>
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
