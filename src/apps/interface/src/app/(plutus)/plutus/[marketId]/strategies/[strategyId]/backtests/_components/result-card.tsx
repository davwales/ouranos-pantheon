import { Card, CardContent } from "@/components/ui/card";

export function ResultCard({
  label,
  value,
  color,
}: {
  label: string;
  value: string;
  color?: string;
}) {
  return (
    <Card>
      <CardContent className="pt-6">
        <div className="text-sm text-muted-foreground">{label}</div>
        <div className={`text-2xl font-bold mt-1 ${color ?? ""}`}>{value}</div>
      </CardContent>
    </Card>
  );
}
