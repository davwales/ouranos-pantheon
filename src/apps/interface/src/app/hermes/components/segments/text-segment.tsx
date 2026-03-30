import { MarkdownRenderer } from "@/app/components/markdown-renderer";
import { memo } from "react";

export const TextSegment = memo(function TextSegment({
  content,
}: {
  content: string;
}) {
  return (
    <MarkdownRenderer componentClassName={{ blockCode: "my-4" }}>
      {content}
    </MarkdownRenderer>
  );
});
