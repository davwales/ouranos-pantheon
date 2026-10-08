"use client";

import {
  AssistantPanel,
  type AssistantPanelProps,
} from "@/components/shared/ai-assistant/assistant-panel";
import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { Sparkles } from "lucide-react";
import { useState } from "react";

export type AssistantButtonProps<TContext> = Omit<
  AssistantPanelProps<TContext>,
  "trigger" | "open" | "onOpenChange"
>;

export function AssistantButton<TContext>(
  props: AssistantButtonProps<TContext>,
) {
  const [open, setOpen] = useState(false);
  const label = `Open ${props.title}`;

  return (
    <>
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              variant="ghost"
              size="icon"
              aria-label={label}
              onClick={() => setOpen(true)}
            >
              <Sparkles />
            </Button>
          </TooltipTrigger>
          <TooltipContent>{props.title}</TooltipContent>
        </Tooltip>
      </TooltipProvider>
      <AssistantPanel {...props} open={open} onOpenChange={setOpen} />
    </>
  );
}
