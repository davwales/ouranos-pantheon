import { NumericInput } from "@/components/shared/numeric-input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { type SwingConfiguration } from "@/lib/api/plutus";
import { THRESHOLD_FIELDS } from "./strategy-constants";

type ThresholdsCardProps = {
  configuration: SwingConfiguration;
  onChange: (next: SwingConfiguration) => void;
};

export function ThresholdsCard({ configuration, onChange }: ThresholdsCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Thresholds</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {THRESHOLD_FIELDS.map((field) => (
            <NumericInput
              key={field.key}
              id={`threshold-${field.key}`}
              label={field.label}
              hint={field.hint}
              placeholder={field.placeholder}
              value={configuration[field.key]}
              onChange={(v) => onChange({ ...configuration, [field.key]: v })}
              min={field.min}
              max={field.max}
              step={field.step}
              aria-label={field.label}
            />
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
