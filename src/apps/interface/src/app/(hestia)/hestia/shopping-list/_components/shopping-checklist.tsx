"use client";

import { Button } from "@/components/ui/button";
import type { ShoppingCategory } from "@/lib/api/hestia-types";
import { Trash2 } from "lucide-react";
import { CategoryMenu } from "./category-menu";
import { SHOPPING_CATEGORIES, UNCATEGORIZED_LABEL } from "./shopping-category-constants";

export type ChecklistItem = {
  lineId: string;
  label: string;
  // What the category is remembered by: the ingredient name or the manual item text.
  name: string;
  category: ShoppingCategory | null;
  manualItemId?: string;
};

export type ShoppingChecklistProps = {
  items: ChecklistItem[];
  checked: Set<string>;
  onToggle: (lineId: string) => void;
  onDelete: (manualItemId: string) => void;
  onCategoryChange: (name: string, category: ShoppingCategory | null) => void;
};

function groupByCategory(items: ChecklistItem[]) {
  const groups = [
    ...SHOPPING_CATEGORIES.map((c) => ({ key: c.value, label: c.label })),
    { key: null, label: UNCATEGORIZED_LABEL },
  ].map((group) => ({
    ...group,
    items: items.filter((item) => item.category === group.key),
  }));
  return groups.filter((group) => group.items.length > 0);
}

export function ShoppingChecklist({
  items,
  checked,
  onToggle,
  onDelete,
  onCategoryChange,
}: ShoppingChecklistProps) {
  const groups = groupByCategory(items);
  const showHeadings = groups.some((group) => group.key !== null);

  return (
    <div className="space-y-4">
      {groups.map((group) => (
        <section key={group.label} aria-label={showHeadings ? group.label : undefined}>
          {showHeadings && (
            <h3 className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
              {group.label}
            </h3>
          )}
          <ul role="list" className="divide-y divide-border">
            {group.items.map((item) => (
              <ChecklistRow
                key={item.lineId}
                item={item}
                isChecked={checked.has(item.lineId)}
                onToggle={onToggle}
                onDelete={onDelete}
                onCategoryChange={onCategoryChange}
              />
            ))}
          </ul>
        </section>
      ))}
    </div>
  );
}

function ChecklistRow({
  item,
  isChecked,
  onToggle,
  onDelete,
  onCategoryChange,
}: {
  item: ChecklistItem;
  isChecked: boolean;
  onToggle: (lineId: string) => void;
  onDelete: (manualItemId: string) => void;
  onCategoryChange: (name: string, category: ShoppingCategory | null) => void;
}) {
  const { manualItemId } = item;
  return (
    <li
      className={`flex items-center gap-1 py-1 px-1 ${
        isChecked ? "line-through text-muted-foreground" : ""
      }`}
    >
      <label className="flex flex-1 items-center gap-3 py-2 cursor-pointer">
        <input
          type="checkbox"
          className="h-5 w-5 cursor-pointer"
          checked={isChecked}
          onChange={() => onToggle(item.lineId)}
        />
        <span className="text-base">{item.label}</span>
      </label>
      <CategoryMenu
        itemName={item.name}
        category={item.category}
        onChange={(category) => onCategoryChange(item.name, category)}
      />
      {manualItemId && (
        <Button
          variant="ghost"
          size="icon-sm"
          aria-label={`Delete ${item.label}`}
          onClick={() => onDelete(manualItemId)}
        >
          <Trash2 className="size-4" />
        </Button>
      )}
    </li>
  );
}
