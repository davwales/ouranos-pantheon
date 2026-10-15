"use client";

import { Badge } from "@/components/ui/badge";
import { Search, X } from "lucide-react";
import React from "react";
import {
  isSearchGroup,
  OPERATOR_DISPLAY,
} from "../responsive-data-table/filter-group-converters";
import {
  type ExtendedColumnDef,
  type FilterCondition,
  type FilterGroup,
  type FilterGroupItem,
  isFilterCondition,
} from "../responsive-data-table/types";

type FilterChipsProps = {
  group: FilterGroup;
  columns: ExtendedColumnDef<any>[];
  onRemove: (path: number[]) => void;
};

function formatCondition(
  condition: FilterCondition,
  columns: ExtendedColumnDef<any>[],
): string {
  const col = columns.find((c) => c.id === condition.field);
  const fieldName =
    col && typeof col.header === "string" ? col.header : condition.field;

  if (condition.value === null) {
    return condition.operator === "eq"
      ? `${fieldName} is null`
      : `${fieldName} is not null`;
  }
  return `${fieldName} ${OPERATOR_DISPLAY[condition.operator].glyph} ${condition.value}`;
}

function LogicSeparator({ logic }: { logic: "and" | "or" }) {
  return (
    <span className="text-[10px] font-bold uppercase text-muted-foreground">
      {logic}
    </span>
  );
}

function RemovableChip({
  label,
  icon,
  onRemove,
}: {
  label: string;
  icon?: React.ReactNode;
  onRemove: () => void;
}) {
  return (
    <Badge variant="secondary" className="gap-1 pr-1">
      {icon}
      {label}
      <button
        type="button"
        aria-label={`Remove filter ${label}`}
        className="rounded-sm p-0.5 hover:bg-muted-foreground/20"
        onClick={onRemove}
      >
        <X className="h-3 w-3" />
      </button>
    </Badge>
  );
}

function ChipItem({
  item,
  path,
  columns,
  onRemove,
}: {
  item: FilterGroupItem;
  path: number[];
  columns: ExtendedColumnDef<any>[];
  onRemove: (path: number[]) => void;
}): React.ReactNode {
  if (isFilterCondition(item)) {
    return (
      <RemovableChip
        label={formatCondition(item, columns)}
        onRemove={() => onRemove(path)}
      />
    );
  }

  if (isSearchGroup(item)) {
    return (
      <RemovableChip
        label={`“${item.search}”`}
        icon={<Search className="h-3 w-3" />}
        onRemove={() => onRemove(path)}
      />
    );
  }

  return (
    <span className="inline-flex flex-wrap items-center gap-1 rounded-md border border-input px-1.5 py-0.5">
      <ChipList group={item} path={path} columns={columns} onRemove={onRemove} />
    </span>
  );
}

function ChipList({
  group,
  path,
  columns,
  onRemove,
}: {
  group: FilterGroup;
  path: number[];
  columns: ExtendedColumnDef<any>[];
  onRemove: (path: number[]) => void;
}) {
  return group.items.map((item, index) => (
    <React.Fragment key={index}>
      {index > 0 && <LogicSeparator logic={group.logic} />}
      <ChipItem
        item={item}
        path={[...path, index]}
        columns={columns}
        onRemove={onRemove}
      />
    </React.Fragment>
  ));
}

export function FilterChips({ group, columns, onRemove }: FilterChipsProps) {
  if (group.items.length === 0) return null;

  return (
    <div className="mt-2 flex flex-wrap items-center gap-1.5">
      <ChipList group={group} path={[]} columns={columns} onRemove={onRemove} />
    </div>
  );
}
