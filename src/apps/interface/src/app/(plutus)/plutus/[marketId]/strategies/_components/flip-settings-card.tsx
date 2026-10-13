import { NumericInput } from "@/components/shared/numeric-input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { type FlipConfiguration } from "@/lib/api/plutus";
import { FLIP_FIELDS } from "./flip-constants";
import { LockToggle } from "./lock-toggle";
import { isLocked, withLock } from "./parameter-locks";

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
              labelAction={
                <LockToggle
                  label={field.label}
                  locked={isLocked(configuration, field.key)}
                  onToggle={
                    field.isOptimized
                      ? () =>
                          onChange(
                            withLock(
                              configuration,
                              field.key,
                              !isLocked(configuration, field.key),
                            ),
                          )
                      : undefined
                  }
                />
              }
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
