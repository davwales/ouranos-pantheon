import ClipboardCopy from "@/components/shared/clipboard-copy";
import { Check, Copy } from "lucide-react";

export function JsonView({
  label,
  value,
  emptyText,
  footer,
}: {
  label: string;
  value: string | undefined;
  emptyText: string;
  footer?: React.ReactNode;
}) {
  const formatted = format(value);

  return (
    <section aria-label={label} className="min-w-0 overflow-hidden rounded-md border bg-muted/40">
      <div className="flex h-7 items-center justify-between gap-2 border-b px-2.5">
        <span className="text-[11px] font-medium tracking-wide text-muted-foreground uppercase">
          {label}
        </span>
        {formatted && (
          <ClipboardCopy
            value={formatted}
            hideIfNotSupported
            confirmation={
              <span className="flex size-5 items-center justify-center">
                <Check aria-label="Copied" className="size-3.5 text-emerald-600" />
              </span>
            }
          >
            <button
              type="button"
              aria-label={`Copy ${label.toLowerCase()}`}
              className="flex size-5 cursor-pointer items-center justify-center rounded text-muted-foreground outline-none hover:text-foreground focus-visible:ring-[3px] focus-visible:ring-ring/50"
            >
              <Copy className="size-3.5" />
            </button>
          </ClipboardCopy>
        )}
      </div>
      {formatted ? (
        <pre className="max-h-64 overflow-auto px-2.5 py-2 font-mono text-xs leading-relaxed whitespace-pre-wrap wrap-anywhere text-foreground/90">
          {formatted}
        </pre>
      ) : (
        <p className="px-2.5 py-2 text-xs text-muted-foreground italic">{emptyText}</p>
      )}
      {footer && (
        <p className="border-t px-2.5 py-1.5 text-xs text-muted-foreground">{footer}</p>
      )}
    </section>
  );
}

function format(value: string | undefined): string {
  const text = value?.trim() ?? "";
  if (text.length === 0) {
    return "";
  }

  try {
    const parsed: unknown = JSON.parse(text);
    if (parsed !== null && typeof parsed === "object" && Object.keys(parsed).length === 0) {
      return "";
    }
    return JSON.stringify(parsed, null, 2);
  } catch {
    return text.startsWith("{") || text.startsWith("[") ? indentPartialJson(text) : text;
  }
}

// Tool outputs are cut to what the model saw, which leaves JSON that cannot be parsed, so it is
// indented token by token instead.
function indentPartialJson(text: string): string {
  let result = "";
  let depth = 0;
  let inString = false;
  let isEscaped = false;
  const newline = () => `\n${"  ".repeat(depth)}`;

  for (const char of text) {
    if (inString) {
      result += char;
      if (isEscaped) {
        isEscaped = false;
      } else if (char === "\\") {
        isEscaped = true;
      } else if (char === '"') {
        inString = false;
      }
      continue;
    }

    switch (char) {
      case '"':
        inString = true;
        result += char;
        break;
      case "{":
      case "[":
        depth++;
        result += char + newline();
        break;
      case "}":
      case "]":
        depth = Math.max(0, depth - 1);
        result += newline() + char;
        break;
      case ",":
        result += char + newline();
        break;
      case ":":
        result += ": ";
        break;
      default:
        if (!/\s/.test(char)) {
          result += char;
        }
    }
  }
  return result;
}
