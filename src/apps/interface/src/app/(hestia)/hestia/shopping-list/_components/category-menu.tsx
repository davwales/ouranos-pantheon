"use client";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { ShoppingCategory } from "@/lib/api/hestia-types";
import { Tag } from "lucide-react";
import { SHOPPING_CATEGORIES, UNCATEGORIZED_LABEL } from "./shopping-category-constants";

const NONE = "none";

export type CategoryMenuProps = {
  itemName: string;
  category: ShoppingCategory | null;
  onChange: (category: ShoppingCategory | null) => void;
};

export function CategoryMenu({ itemName, category, onChange }: CategoryMenuProps) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          size="icon-sm"
          aria-label={`Set category for ${itemName}`}
          className={category ? "text-muted-foreground" : "text-muted-foreground/50"}
        >
          <Tag className="size-4" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuLabel>Category</DropdownMenuLabel>
        <DropdownMenuRadioGroup
          value={category ?? NONE}
          onValueChange={(value) =>
            onChange(value === NONE ? null : (value as ShoppingCategory))
          }
        >
          {SHOPPING_CATEGORIES.map((option) => (
            <DropdownMenuRadioItem key={option.value} value={option.value}>
              {option.label}
            </DropdownMenuRadioItem>
          ))}
          <DropdownMenuRadioItem value={NONE}>{UNCATEGORIZED_LABEL}</DropdownMenuRadioItem>
        </DropdownMenuRadioGroup>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
