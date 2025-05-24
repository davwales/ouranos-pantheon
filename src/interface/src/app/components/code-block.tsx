import ClipboardCopy from "@/app/components/clipboard-copy";
import { Copy } from "lucide-react";
import React from "react";
import SyntaxHighlighter from "react-syntax-highlighter";
import { dracula } from "react-syntax-highlighter/dist/esm/styles/hljs";

export function CodeBlock({
  code,
  language = "text",
  syntaxProps,
  className,
  ...props
}: React.ComponentProps<"div"> & {
  language?: string | undefined;
  code: string;
  syntaxProps?: React.ComponentProps<typeof SyntaxHighlighter>;
}) {
  return (
    <div
      className={`bg-background rounded-lg border shadow-lg ${className}`}
      {...props}
    >
      <div className="flex justify-between items-center px-4 py-3">
        <span className="text-xs font-mono uppercase tracking-wide">
          {language || "Code"}
        </span>

        <ClipboardCopy value={code} hideIfNotSupported>
          <Copy size={16} />
        </ClipboardCopy>
      </div>

      <SyntaxHighlighter
        style={dracula}
        customStyle={{ margin: 0 }}
        wrapLongLines
        PreTag="div"
        language={language}
        {...syntaxProps}
      >
        {code}
      </SyntaxHighlighter>
    </div>
  );
}
