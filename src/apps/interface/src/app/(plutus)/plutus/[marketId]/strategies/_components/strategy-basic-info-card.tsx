import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { FieldLabel } from "./field-label";

type StrategyBasicInfoCardProps = {
  name: string;
  description: string;
  onNameChange: (name: string) => void;
  onDescriptionChange: (description: string) => void;
};

export function StrategyBasicInfoCard({
  name,
  description,
  onNameChange,
  onDescriptionChange,
}: StrategyBasicInfoCardProps) {
  const nameId = "strategy-name";
  const descriptionId = "strategy-description";

  return (
    <Card>
      <CardHeader>
        <CardTitle>Basic Information</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-1">
          <FieldLabel htmlFor={nameId}>Name</FieldLabel>
          <Input
            id={nameId}
            value={name}
            onChange={(e) => onNameChange(e.target.value)}
            placeholder="e.g. Aggressive Signal Strategy"
          />
        </div>
        <div className="space-y-1">
          <FieldLabel htmlFor={descriptionId}>Description (optional)</FieldLabel>
          <Textarea
            id={descriptionId}
            value={description}
            onChange={(e) => onDescriptionChange(e.target.value)}
            placeholder="Describe the strategy's goals and parameters"
            rows={3}
          />
        </div>
      </CardContent>
    </Card>
  );
}
