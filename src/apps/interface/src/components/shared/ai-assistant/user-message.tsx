import { TextBlock } from "@/components/shared/ai-assistant/blocks/text-block";
import { cn } from "@/lib/utils";

export function UserMessage({
  content,
  className,
  ...props
}: React.ComponentProps<"div"> & { content: string }) {
  return (
    <div
      className={cn(
        "ml-auto w-fit max-w-[85%] min-w-0 rounded-2xl rounded-br-md bg-muted px-4 py-2 text-left wrap-break-word",
        className,
      )}
      {...props}
    >
      <TextBlock content={content} />
    </div>
  );
}
