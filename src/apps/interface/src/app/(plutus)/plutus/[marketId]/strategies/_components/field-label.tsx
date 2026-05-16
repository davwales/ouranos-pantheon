export function FieldLabel({
  children,
  hint,
}: {
  children: React.ReactNode;
  hint?: string;
}) {
  return (
    <label className="text-sm font-medium block">
      {children}
      {hint && (
        <span className="text-muted-foreground text-xs ml-1">({hint})</span>
      )}
    </label>
  );
}
