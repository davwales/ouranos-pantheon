"use client";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Plus } from "lucide-react";
import React from "react";
import { FilterInput } from "./filter-input";
import {
  type ExtendedColumnDef,
  type FilterCondition,
  type FilterGroup,
  type FilterGroupItem,
  isFilterCondition,
} from "./types";

const MAX_DEPTH = 3;

interface FilterGroupBuilderProps {
  group: FilterGroup;
  columns: ExtendedColumnDef<any>[];
  onChange: (group: FilterGroup) => void;
  onRemove?: () => void;
  depth: number;
}

interface GroupActionRowProps {
  onAddCondition: () => void;
  onAddGroup: () => void;
  columns: ExtendedColumnDef<any>[];
  depth: number;
}

function createEmptyCondition(
  columns: ExtendedColumnDef<any>[],
): FilterCondition {
  const column = columns.find((col) => col.filterConfig);
  return {
    field: column?.id ?? "",
    operator: column?.filterConfig?.operators[0] ?? "eq",
    value: "",
  };
}

export function addCondition(
  group: FilterGroup,
  columns: ExtendedColumnDef<any>[],
): FilterGroup {
  return {
    ...group,
    items: [...group.items, createEmptyCondition(columns)],
  };
}

export function addGroup(group: FilterGroup): FilterGroup {
  return {
    ...group,
    items: [...group.items, { logic: "and", items: [] }],
  };
}

export function updateCondition(
  group: FilterGroup,
  index: number,
  condition: FilterCondition,
): FilterGroup {
  const items = [...group.items];
  items[index] = condition;
  return { ...group, items };
}

export function removeItem(group: FilterGroup, index: number): FilterGroup {
  return {
    ...group,
    items: group.items.filter((_, i) => i !== index),
  };
}

export function updateItem(
  group: FilterGroup,
  index: number,
  item: FilterGroupItem,
): FilterGroup {
  const items = [...group.items];
  items[index] = item;
  return { ...group, items };
}

function GroupLogicSelector({
  value,
  onChange,
}: {
  value: "and" | "or";
  onChange: (logic: "and" | "or") => void;
}) {
  return (
    <div
      className="inline-flex items-center rounded-md border border-input bg-muted p-0.5"
      role="tablist"
      aria-label="Group logic"
    >
      {(["and", "or"] as const).map((logic) => {
        const isActive = value === logic;
        return (
          <Button
            key={logic}
            type="button"
            variant="ghost"
            size="sm"
            role="tab"
            aria-selected={isActive}
            className={cn(
              "h-6 px-3 text-xs font-semibold uppercase",
              isActive && "bg-background text-foreground shadow-sm",
            )}
            onClick={() => onChange(logic)}
          >
            {logic}
          </Button>
        );
      })}
    </div>
  );
}

function GroupActionRow({
  onAddCondition,
  onAddGroup,
  columns,
  depth,
}: GroupActionRowProps) {
  const hasFilterable = columns.some((col) => col.filterConfig);
  const atMaxDepth = depth >= MAX_DEPTH;

  return (
    <div className="flex flex-wrap gap-2 mt-3">
      <Button
        variant="outline"
        size="sm"
        onClick={onAddCondition}
        disabled={!hasFilterable}
      >
        Add Condition <Plus className="ml-2 h-4 w-4" />
      </Button>
      {atMaxDepth ? (
        <Badge variant="secondary">Max nesting reached</Badge>
      ) : (
        <Button variant="outline" size="sm" onClick={onAddGroup}>
          Add Group <Plus className="ml-2 h-4 w-4" />
        </Button>
      )}
    </div>
  );
}

export function FilterGroupBuilder({
  group,
  columns,
  onChange,
  onRemove,
  depth,
}: FilterGroupBuilderProps) {
  const handleLogicChange = (logic: "and" | "or") => {
    onChange({ ...group, logic });
  };

  const handleAddCondition = () => {
    onChange(addCondition(group, columns));
  };

  const handleAddGroup = () => {
    onChange(addGroup(group));
  };

  const children = group.items.map((item, index) => (
    <React.Fragment key={index}>
      {isFilterCondition(item) ? (
        <FilterInput
          columns={columns}
          value={item}
          onChange={(updated) =>
            onChange(updateCondition(group, index, updated))
          }
          onRemove={() => onChange(removeItem(group, index))}
        />
      ) : (
        <FilterGroupBuilder
          group={item}
          columns={columns}
          depth={depth + 1}
          onChange={(updated) => onChange(updateItem(group, index, updated))}
          onRemove={() => onChange(removeItem(group, index))}
        />
      )}
    </React.Fragment>
  ));

  const isRoot = depth === 0;
  const isEmpty = group.items.length === 0;

  const shellClass = cn(
    "border border-input rounded-lg p-3",
    isRoot
      ? "bg-card shadow-sm"
      : cn("my-2 bg-muted/40 border-l-4 border-l-primary", isEmpty && "border-dashed"),
  );

  return (
    <div
      className={shellClass}
      role="group"
      aria-label={group.logic === "and" ? "AND group" : "OR group"}
    >
      <div className="flex items-center justify-between mb-2">
        {isRoot ? (
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">Match</span>
            <GroupLogicSelector
              value={group.logic}
              onChange={handleLogicChange}
            />
          </div>
        ) : (
          <GroupLogicSelector value={group.logic} onChange={handleLogicChange} />
        )}
        {onRemove && (
          <Button
            variant="ghost"
            size="sm"
            onClick={onRemove}
            className="text-destructive h-6 px-2"
          >
            Remove Group
          </Button>
        )}
      </div>
      {isEmpty ? (
        <p className="text-sm text-muted-foreground italic py-2">
          This group is empty. Add a condition or another group.
        </p>
      ) : (
        <div className="flex flex-col gap-3">{children}</div>
      )}
      <GroupActionRow
        onAddCondition={handleAddCondition}
        onAddGroup={handleAddGroup}
        columns={columns}
        depth={depth}
      />
    </div>
  );
}
