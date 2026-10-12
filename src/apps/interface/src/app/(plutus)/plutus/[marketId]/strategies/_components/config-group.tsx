import { Typography } from "@/components/shared/typography";

export function ConfigGroup({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-2">
      <Typography
        variant="small"
        className="font-semibold uppercase tracking-wide text-muted-foreground"
      >
        {title}
      </Typography>
      <div className="space-y-1">{children}</div>
    </div>
  );
}
