"use client";

import { Button } from "@/components/ui/button";
import type { ManualItem } from "@/lib/api/hestia-types";
import { Trash2 } from "lucide-react";

export type ManualItemsListProps = {
  items: ManualItem[];
  checked: Set<string>;
  onToggle: (lineId: string) => void;
  onDelete: (itemId: string) => void;
};

export function ManualItemsList({
  items,
  checked,
  onToggle,
  onDelete,
}: ManualItemsListProps) {
  return (
    <ul role="list" className="divide-y divide-border">
      {items.map((item) => {
        const lineId = `manual:${item.id}`;
        const isChecked = checked.has(lineId);

        return (
          <li key={item.id}>
            <div
              className={`flex items-center justify-between gap-3 py-3 px-1 ${
                isChecked ? "line-through text-muted-foreground" : ""
              }`}
            >
              <label className="flex flex-1 items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  className="h-5 w-5 cursor-pointer"
                  checked={isChecked}
                  onChange={() => onToggle(lineId)}
                />
                <span className="text-base">{item.text}</span>
              </label>
              <Button
                variant="ghost"
                size="icon-sm"
                aria-label={`Delete ${item.text}`}
                onClick={() => onDelete(item.id)}
              >
                <Trash2 className="size-4" />
              </Button>
            </div>
          </li>
        );
      })}
    </ul>
  );
}
