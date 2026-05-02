export function InfoChip({ label, value }: { label: string; value: string }) {
  return (
    <span className="inline-flex items-center gap-1 rounded-full border bg-muted/50 px-2.5 py-1 text-xs font-medium">
      <span className="text-muted-foreground">{label}:</span>
      {value}
    </span>
  );
}
