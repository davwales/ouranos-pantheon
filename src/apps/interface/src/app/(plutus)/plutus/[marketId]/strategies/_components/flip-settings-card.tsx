import { NumericInput } from "@/components/shared/numeric-input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { type FlipConfiguration } from "@/lib/api/plutus";
import { FLIP_FIELDS } from "./flip-constants";

type FlipSettingsCardProps = {
  configuration: FlipConfiguration;
  onChange: (next: FlipConfiguration) => void;
};

export function FlipSettingsCard({
  configuration,
  onChange,
}: FlipSettingsCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Flip Settings</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {FLIP_FIELDS.map((field) => (
            <NumericInput
              key={field.key}
              label={field.label}
              hint={field.hint}
              value={configuration[field.key]}
              onChange={(v) =>
                onChange({ ...configuration, [field.key]: v ?? 0 })
              }
              min={field.min}
              max={field.max}
              step={field.step}
            />
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
