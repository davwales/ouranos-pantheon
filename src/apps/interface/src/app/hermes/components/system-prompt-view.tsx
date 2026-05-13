"use client";

import { MarkdownRenderer } from "@/app/components/markdown-renderer";
import ClipboardCopy from "@/app/components/clipboard-copy";
import { Button } from "@/components/ui/button";
import { Copy, Eye, FileText } from "lucide-react";
import { useState } from "react";

type ViewMode = "rendered" | "raw";

export function SystemPromptView({ content }: { content: string }) {
  const [mode, setMode] = useState<ViewMode>("rendered");

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-1">
          <Button
            variant={mode === "rendered" ? "secondary" : "ghost"}
            size="sm"
            className="h-7 px-2 text-xs gap-1"
            onClick={() => setMode("rendered")}
          >
            <Eye className="h-3.5 w-3.5" />
            Rendered
          </Button>
          <Button
            variant={mode === "raw" ? "secondary" : "ghost"}
            size="sm"
            className="h-7 px-2 text-xs gap-1"
            onClick={() => setMode("raw")}
          >
            <FileText className="h-3.5 w-3.5" />
            Raw
          </Button>
        </div>

        <ClipboardCopy
          value={content}
          hideIfNotSupported
          className="p-1.5 hover:bg-accent rounded-md transition-colors"
        >
          <Copy className="h-4 w-4 text-muted-foreground hover:text-foreground" />
        </ClipboardCopy>
      </div>

      <div className="bg-muted/50 border rounded-md px-3 py-2 overflow-y-auto max-h-[50vh] min-h-32">
        {mode === "rendered" ? (
          <div className="text-sm leading-relaxed [&_h1]:text-base [&_h1]:font-semibold [&_h1]:mt-4 [&_h1]:mb-2 [&_h2]:text-sm [&_h2]:font-semibold [&_h2]:mt-3 [&_h2]:mb-1 [&_h3]:text-sm [&_h3]:font-medium [&_h3]:mt-3 [&_h3]:mb-1 [&_p]:mb-2 [&_ul]:space-y-1 [&_ul]:mb-2 [&_ol]:space-y-1 [&_ol]:mb-2 [&_li]:ml-2">
            <MarkdownRenderer componentClassName={{ blockCode: "my-2" }}>
              {content}
            </MarkdownRenderer>
          </div>
        ) : (
          <pre className="text-sm font-mono whitespace-pre-wrap break-all leading-relaxed">
            {content}
          </pre>
        )}
      </div>
    </div>
  );
}
