"use client";

import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { useDebounce } from "@/hooks/use-debounce";
import React, { useMemo } from "react";
import { OPERATOR_DISPLAY, queryToFilterGroup } from "../responsive-data-table/filter-group-converters";
import {
  type ExtendedColumnDef,
  type FilterCondition,
  type FilterGroup,
  type FilterGroupItem,
  type FilterOperator,
  isFilterCondition,
} from "../responsive-data-table/types";

function formatCondition(
  condition: FilterCondition,
  columns: ExtendedColumnDef<any>[],
): string {
  const opLabel =
    OPERATOR_DISPLAY[condition.operator].glyph ?? condition.operator;
  const col = columns.find((c) => c.id === condition.field);
  const fieldName =
    col && typeof col.header === "string" ? col.header : condition.field;

  if (condition.value === null) {
    return condition.operator === "eq"
      ? `${fieldName} is null`
      : `${fieldName} is not null`;
  }
  return `${fieldName} ${opLabel} ${condition.value}`;
}

function AndSeparator() {
  return (
    <span className="text-xs font-bold text-muted-foreground bg-muted rounded px-2 py-0.5">
      AND
    </span>
  );
}

function OrSeparator() {
  return (
    <span className="text-xs font-bold text-muted-foreground bg-muted rounded px-2 py-0.5">
      OR
    </span>
  );
}

function FilterPreview({
  group,
  depth = 0,
  columns,
}: {
  group: FilterGroup;
  depth?: number;
  columns: ExtendedColumnDef<any>[];
}): React.ReactNode {
  if (group.items.length === 0) return null;

  const children = group.items.map((item, index) => (
    <React.Fragment key={index}>
      <FilterPreviewItem item={item} depth={depth + 1} columns={columns} />
      {index < group.items.length - 1 &&
        (group.logic === "or" ? <OrSeparator /> : <AndSeparator />)}
    </React.Fragment>
  ));

  if (group.logic === "or") {
    return (
      <span className="inline-flex flex-wrap items-center gap-2 border border-input rounded-lg px-2 py-1 bg-muted/20">
        {children}
      </span>
    );
  }

  if (depth > 0) {
    return (
      <span className="inline-flex flex-wrap items-center gap-2 border border-input rounded-lg px-2 py-1 bg-muted/40 border-l-4 border-l-primary">
        {children}
      </span>
    );
  }

  return (
    <div className="inline-flex flex-wrap items-center gap-2 rounded-lg bg-muted/30 p-2">
      {children}
    </div>
  );
}

function FilterPreviewItem({
  item,
  depth,
  columns,
}: {
  item: FilterGroupItem;
  depth: number;
  columns: ExtendedColumnDef<any>[];
}): React.ReactNode {
  if (isFilterCondition(item)) {
    return <Badge variant="secondary">{formatCondition(item, columns)}</Badge>;
  }

  return <FilterPreview group={item} depth={depth} columns={columns} />;
}

type SmartFilterInputProps = {
  columns: ExtendedColumnDef<any>[];
  value: string;
  onChange: (group: FilterGroup | null) => void;
  onQueryChange: (query: string) => void;
  className?: string;
};

export function SmartFilterInput({
  columns,
  value,
  onChange,
  onQueryChange,
  className,
}: SmartFilterInputProps) {
  const debouncedQuery = useDebounce(value);

  const liveResult = useMemo(
    () => queryToFilterGroup(value, columns),
    [value, columns],
  );

  const debouncedResult = useMemo(
    () => queryToFilterGroup(debouncedQuery, columns),
    [debouncedQuery, columns],
  );

  const onChangeRef = React.useRef(onChange);

  React.useEffect(() => {
    onChangeRef.current = onChange;
  });

  React.useEffect(() => {
    onChangeRef.current(debouncedResult);
  }, [debouncedResult]);

  const placeholder = useMemo(() => {
    const filterable = columns.filter((c) => c.filterConfig);
    const nameA =
      typeof filterable[0]?.header === "string"
        ? filterable[0].header
        : filterable[0]?.id ?? "name";
    const nameB =
      typeof filterable[1]?.header === "string"
        ? filterable[1].header
        : filterable[1]?.id ?? "limit";
    return `Try "${nameA} contains sword" or "${nameB} >= 100"`;
  }, [columns]);

  const errorId = "smart-filter-error";
  const hasError = value.trim().length > 0 && liveResult === null;

  return (
    <div className={className}>
      <Input
        type="text"
        value={value}
        onChange={(e) => onQueryChange(e.target.value)}
        placeholder={placeholder}
        aria-label="Smart filter query"
        aria-invalid={hasError}
        aria-describedby={hasError ? errorId : undefined}
      />
      {liveResult && liveResult.items.length > 0 && (
        <div className="mt-2 flex flex-wrap items-center gap-2">
          <FilterPreview group={liveResult} depth={0} columns={columns} />
        </div>
      )}
      {hasError && (
        <p
          id={errorId}
          role="alert"
          aria-live="polite"
          className="mt-2 text-sm text-destructive"
        >
          Invalid filter query
        </p>
      )}
    </div>
  );
}
