import { JsonView } from "@/components/shared/ai-assistant/blocks/json-view";

export function ToolDetails({
  context,
  input,
  output,
  isTruncated,
  isSettled,
}: {
  context?: unknown;
  input: string;
  output: string | undefined;
  isTruncated?: boolean;
  isSettled: boolean;
}) {
  return (
    <div className="flex min-w-0 flex-col gap-2">
      <JsonView label="Input" value={withContext(context, input)} emptyText="No arguments" />
      <JsonView
        label="Output"
        value={output}
        emptyText={isSettled ? "No output" : "Waiting for the result…"}
        footer={isTruncated ? "Truncated: the assistant only saw this much." : undefined}
      />
    </div>
  );
}

// A tool runs with the page's context as well as the model's arguments, so both are shown, or the
// input of a call such as get_positions would not say which symbol it was for.
function withContext(context: unknown, input: string): string {
  if (context === undefined) {
    return input;
  }

  const text = input.trim();
  let args: unknown = {};
  if (text.length > 0) {
    try {
      args = JSON.parse(text);
    } catch {
      args = text;
    }
  }
  return JSON.stringify({ context, arguments: args });
}
