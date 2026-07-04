"use client";

import { cn } from "@/lib/utils";
import React, { useId } from "react";

type RecipeFormFieldProps = {
  label: string;
  required?: boolean;
  error?: string;
  helperText?: string;
  children: React.ReactElement<{
    id?: string;
    "aria-invalid"?: "true" | "false";
    "aria-describedby"?: string;
    required?: boolean;
  }>;
  className?: string;
};

export function RecipeFormField({
  label,
  required = false,
  error,
  helperText,
  children,
  className,
}: RecipeFormFieldProps) {
  const generatedId = useId();
  const childId = children.props.id ?? generatedId;
  const errorId = `${childId}-error`;
  const helperId = `${childId}-helper`;
  const describedBy = [error ? errorId : null, helperText ? helperId : null]
    .filter(Boolean)
    .join(" ") || undefined;

  return (
    <div className={cn("space-y-2", className)}>
      <label htmlFor={childId} className="text-sm font-medium">
        {label}
        {required && <span aria-hidden="true"> *</span>}
      </label>
      {React.cloneElement(children, {
        id: childId,
        "aria-invalid": error ? "true" : "false",
        "aria-describedby": describedBy,
        required: required ? true : undefined,
      })}
      {helperText && (
        <p id={helperId} className="text-sm text-muted-foreground">
          {helperText}
        </p>
      )}
      {error && (
        <p id={errorId} className="text-sm text-destructive" role="alert">
          {error}
        </p>
      )}
    </div>
  );
}
