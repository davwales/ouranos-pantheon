import { MarkdownRenderer } from "@/components/shared/markdown-renderer";
import { memo } from "react";

export const TextBlock = memo(function TextBlock({
  content,
}: {
  content: string;
}) {
  return (
    <MarkdownRenderer
      variant="compact"
      componentClassName={{ blockCode: "my-4" }}
    >
      {content}
    </MarkdownRenderer>
  );
});
