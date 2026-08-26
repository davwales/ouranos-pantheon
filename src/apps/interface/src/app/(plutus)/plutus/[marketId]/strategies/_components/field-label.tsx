export function FieldLabel({
  children,
  hint,
  htmlFor,
}: {
  children: React.ReactNode;
  hint?: string;
  htmlFor?: string;
}) {
  return (
    <label htmlFor={htmlFor} className="text-sm font-medium block">
      {children}
      {hint && (
        <span className="text-muted-foreground text-xs ml-1">({hint})</span>
      )}
    </label>
  );
}
