import { Typography } from "@/components/shared/typography";
import { LoadingSegment } from "@/app/(hermes)/hermes/components/segments/loading-segment";
import { TextSegment } from "@/app/(hermes)/hermes/components/segments/text-segment";
import { ThinkingSegment } from "@/app/(hermes)/hermes/components/segments/thinking-segment";
import { parseSegments } from "@/app/(hermes)/hermes/utils/parse-segments";
import { Role } from "@/lib/api/hermes";

export function Message({
  name,
  role,
  content,
  isStreaming = false,
  ...props
}: React.ComponentProps<"div"> & {
  name: string;
  role: Role;
  content: string;
  isStreaming?: boolean;
}) {
  const segments =
    role === Role.User
      ? [{ type: "text" as const, content }]
      : parseSegments(content, isStreaming);

  return (
    <div {...props}>
      <div
        className={`py-2 px-4 border rounded-2xl ${
          role == Role.User ? "bg-accent/30" : ""
        }`}
      >
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
        className={`mx-2.5 my-1 ${role == Role.User && "text-right"}`}
      >
        {name}
      </Typography>
    </div>
  );
}
