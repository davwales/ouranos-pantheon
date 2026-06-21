"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useDebounce } from "@/hooks/use-debounce";
import React, { useEffect, useLayoutEffect, useState } from "react";
import {
  type ExtendedColumnDef,
  type FilterCondition,
  type FilterOperator,
  OPERATOR_LABELS,
} from "./types";

interface FilterInputProps {
  columns: ExtendedColumnDef<any>[];
  value: FilterCondition;
  onChange: (value: FilterCondition) => void;
  onRemove: () => void;
}

export function FilterInput({
  columns,
  value,
  onChange,
  onRemove,
}: FilterInputProps) {
  const filterableColumns = columns.filter((col) => col.filterConfig);
  const currentColumn = columns.find((col) => col.id === value.field);

  const [textInputValue, setTextInputValue] = useState(
    String(value.value ?? ""),
  );
  const debouncedTextValue = useDebounce(textInputValue);

  const onChangeRef = React.useRef(onChange);
  const valueRef = React.useRef(value);
  const filterTypeRef = React.useRef(currentColumn?.filterConfig?.type);

  useLayoutEffect(() => {
    onChangeRef.current = onChange;
    valueRef.current = value;
    filterTypeRef.current = currentColumn?.filterConfig?.type;
  });

  useEffect(() => {
    if (filterTypeRef.current === "number") {
      const parsed = parseFloat(debouncedTextValue);
      if (!Number.isNaN(parsed))
        onChangeRef.current({ ...valueRef.current, value: parsed });
    } else if (
      filterTypeRef.current !== "boolean" &&
      filterTypeRef.current !== "enum" &&
      filterTypeRef.current !== "date"
    ) {
      onChangeRef.current({ ...valueRef.current, value: debouncedTextValue });
    }
  }, [debouncedTextValue]);

  const handleColumnChange = (columnId: string) => {
    const column = columns.find((col) => col.id === columnId);
    if (!column?.filterConfig) return;

    onChange({
      field: columnId,
      operator: column.filterConfig.operators[0],
      value: "",
    });
  };

  const handleOperatorChange = (operator: FilterOperator) => {
    onChange({ ...value, operator });
  };

  const handleValueChange = (newValue: string) => {
    let parsedValue: any = newValue;

    switch (currentColumn?.filterConfig?.type) {
      case "number":
        parsedValue = parseFloat(newValue);
        break;
      case "boolean":
        parsedValue = newValue === "true";
        break;
      case "date":
        parsedValue = new Date(newValue).toISOString();
        break;
    }

    onChange({ ...value, value: parsedValue });
  };

  if (!currentColumn?.filterConfig) return null;

  return (
    <div className="flex flex-col md:flex-row gap-2 my-2 rounded-lg">
      <Select value={value.field} onValueChange={handleColumnChange}>
        <SelectTrigger className="w-full md:w-48">
          <SelectValue placeholder="Column" />
        </SelectTrigger>
        <SelectContent>
          {filterableColumns.map((col) => (
            <SelectItem key={col.id} value={col.id as string}>
              {col.header as string}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select value={value.operator} onValueChange={handleOperatorChange}>
        <SelectTrigger className="w-full md:w-48">
          <SelectValue placeholder="Operator" />
        </SelectTrigger>
        <SelectContent>
          {currentColumn.filterConfig.operators.map((op: FilterOperator) => (
            <SelectItem key={op} value={op}>
              {OPERATOR_LABELS[op]}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      {currentColumn.filterConfig.type === "enum" ? (
        <Select value={value.value} onValueChange={handleValueChange}>
          <SelectTrigger className="w-full md:w-48">
            <SelectValue placeholder="Value" />
          </SelectTrigger>
          <SelectContent>
            {currentColumn.filterConfig.enumValues?.map((val: string) => (
              <SelectItem key={val} value={val}>
                {val}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      ) : currentColumn.filterConfig.type === "date" ? (
        <Input
          type="date"
          value={value.value}
          onChange={(e) => handleValueChange(e.target.value)}
          className="w-full md:w-48"
        />
      ) : (
        <Input
          type="text"
          value={textInputValue}
          onChange={(e) => setTextInputValue(e.target.value)}
          className="w-full md:w-48"
        />
      )}

      <Button
        variant="destructive"
        onClick={onRemove}
        className="self-end w-full md:w-auto md:self-center"
      >
        Remove
      </Button>
    </div>
  );
}
