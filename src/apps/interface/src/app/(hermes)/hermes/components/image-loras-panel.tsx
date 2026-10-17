"use client";

import { type LoraFormInput } from "@/app/(hermes)/hermes/types";
import { InfoCard } from "@/components/shared/info-card";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { ChevronDown } from "lucide-react";
import Link from "next/link";
import { useState } from "react";

export function ImageLorasPanel({
  activeLoras,
  onLorasChange,
}: {
  activeLoras: LoraFormInput[];
  onLorasChange?: (loras: LoraFormInput[]) => void;
}) {
  const [lorasState] = useApi(() => hermesApi.getAllLoras());
  const [open, setOpen] = useState(false);

  const loras = lorasState.data ?? [];

  const handleToggle = (lora: LoraFormInput) => {
    if (!onLorasChange) return;
    const isActive = activeLoras.some((l) => l.id === lora.id);
    onLorasChange(
      isActive
        ? activeLoras.filter((l) => l.id !== lora.id)
        : [...activeLoras, lora],
    );
  };

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger className="flex items-center justify-between w-full rounded-md px-2 py-2 hover:bg-accent transition-colors">
        <p className="text-sm font-medium">
          Image LoRAs
          {activeLoras.length > 0 && (
            <span className="ml-2 text-muted-foreground">
              {activeLoras.length} active
            </span>
          )}
        </p>
        <ChevronDown
          className={`h-4 w-4 text-muted-foreground transition-transform ${open ? "rotate-180" : ""}`}
        />
      </CollapsibleTrigger>
      <CollapsibleContent className="grid grid-cols-1 gap-3 mt-2">
        {loras.map((lora) => {
          const isActive = activeLoras.some((l) => l.id === lora.id);
          return (
            <InfoCard
              key={lora.id}
              label={lora.name}
              description={`${lora.identifier} · weight ${lora.weight}`}
              onClick={() => handleToggle(lora)}
              className={`hover:bg-accent hover:cursor-pointer w-full ${
                isActive ? "border-accent-foreground" : ""
              }`}
            />
          );
        })}
        {lorasState.status !== "loading" && loras.length === 0 && (
          <p className="px-2 text-sm text-muted-foreground">
            No LoRAs registered.{" "}
            <Link href="/hermes/loras/create" className="underline">
              Register one
            </Link>
          </p>
        )}
      </CollapsibleContent>
    </Collapsible>
  );
}
