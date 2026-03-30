"use client";

import { MarkdownRenderer } from "@/app/components/markdown-renderer";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { Brain, ChevronDown } from "lucide-react";
import { useState } from "react";

export function ThinkingSegment({
  content,
  isStreaming,
}: {
  content: string;
  isStreaming: boolean;
}) {
  const [open, setOpen] = useState(false);

  return (
    <Collapsible open={open} onOpenChange={setOpen} className="mb-2">
      <CollapsibleTrigger asChild>
        <div
          role="button"
          tabIndex={0}
          onClick={(e) => e.stopPropagation()}
          onKeyDown={(e) => {
            if (e.key === "Enter" || e.key === " ") e.stopPropagation();
          }}
          className="flex items-center gap-2 text-muted-foreground text-sm hover:text-foreground transition-colors cursor-pointer"
        >
          <Brain
            className={`h-4 w-4 shrink-0 ${isStreaming ? "animate-pulse" : ""}`}
          />
          <span>{isStreaming ? "Thinking..." : "Reasoning"}</span>
          <ChevronDown
            className={`h-4 w-4 shrink-0 transition-transform ${open ? "rotate-180" : ""}`}
          />
        </div>
      </CollapsibleTrigger>
      <CollapsibleContent>
        <div className="pl-6 mt-2 border-l border-border text-muted-foreground text-sm">
          <MarkdownRenderer componentClassName={{ blockCode: "my-2" }}>
            {content}
          </MarkdownRenderer>
        </div>
      </CollapsibleContent>
    </Collapsible>
  );
}
