import { Wrench } from "lucide-react";

export function ToolSegment({ labels }: { labels: string[] }) {
  return (
    <div className="mb-2 flex items-center gap-2 text-sm text-muted-foreground">
      <Wrench className="h-4 w-4 shrink-0" />
      <span>Looked up {labels.join(", ")}</span>
    </div>
  );
}
