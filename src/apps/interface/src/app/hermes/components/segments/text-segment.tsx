import { MarkdownRenderer } from "@/app/components/markdown-renderer";

export function TextSegment({ content }: { content: string }) {
  return (
    <MarkdownRenderer componentClassName={{ blockCode: "my-4" }}>
      {content}
    </MarkdownRenderer>
  );
}
