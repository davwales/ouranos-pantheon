import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { ImageBlock } from "@/components/shared/ai-assistant/blocks/image-block";
import { TextBlock } from "@/components/shared/ai-assistant/blocks/text-block";
import { cn } from "@/lib/utils";

export function UserMessage({
  content,
  images = [],
  className,
  ...props
}: React.ComponentProps<"div"> & { content: string; images?: AssistantChatImagePart[] }) {
  return (
    <div className={cn("ml-auto flex w-fit max-w-[85%] min-w-0 flex-col items-end gap-2", className)} {...props}>
      {images.map((image) => (
        <ImageBlock key={image.id} image={image} isCompact />
      ))}
      <div className="w-fit max-w-full rounded-2xl rounded-br-md bg-muted px-4 py-2 text-left wrap-break-word">
        <TextBlock content={content} />
      </div>
    </div>
  );
}
