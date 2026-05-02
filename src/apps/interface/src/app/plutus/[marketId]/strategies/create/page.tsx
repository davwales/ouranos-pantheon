"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import {
  type StrategyConfiguration,
  type StrategyType,
  plutusApi,
} from "@/lib/api/plutus";
import { RefreshCw } from "lucide-react";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { NumberInput } from "../components/number-input";
import { StrategyConfigForm } from "../components/strategy-config-form";

const strategyTypes: { value: StrategyType; label: string }[] = [
  { value: "SignalWeighted", label: "Signal Weighted" },
  { value: "ForecastMomentum", label: "Forecast Momentum" },
  { value: "MeanReversion", label: "Mean Reversion" },
  { value: "RecipeArbitrage", label: "Recipe Arbitrage" },
];

function FieldLabel({
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

export default function CreateStrategyPage() {
  const { marketId } = useParams<{ marketId: string }>();
  const router = useRouter();

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [type, setType] = useState<StrategyType>("SignalWeighted");
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [config, setConfig] = useState<StrategyConfiguration>({
    signalWeights: null,
    buyThreshold: null,
    sellThreshold: null,
  });

  const handleSubmit = async () => {
    setIsProcessing(true);
    setError(null);
    try {
      const response = await plutusApi.createStrategy({
        marketId,
        name,
        description: description || null,
        type,
        configuration: config,
      });
      router.replace(`/plutus/${marketId}/strategies/${response.id}`);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to create strategy",
      );
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="space-y-6">
      {error && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
          {error}
        </div>
      )}
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-semibold tracking-tight">
          Create New Strategy
        </h2>
        <div className="flex items-center gap-3">
          {isProcessing && <RefreshCw className="animate-spin size-4" />}
          <Button
            onClick={handleSubmit}
            disabled={isProcessing || !name.trim()}
          >
            Create
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Basic Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-1">
                <FieldLabel>Name</FieldLabel>
                <Input
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="e.g. Aggressive Signal Strategy"
                />
              </div>
              <div className="space-y-1">
                <FieldLabel>Description (optional)</FieldLabel>
                <Textarea
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Describe the strategy's goals and parameters"
                  rows={3}
                />
              </div>
              <div className="space-y-1">
                <FieldLabel>Type</FieldLabel>
                <Select
                  value={type}
                  onValueChange={(value) => {
                    setType(value as StrategyType);
                    setConfig({});
                  }}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Select strategy type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectGroup>
                      {strategyTypes.map((t) => (
                        <SelectItem key={t.value} value={t.value}>
                          {t.label}
                        </SelectItem>
                      ))}
                    </SelectGroup>
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Configuration</CardTitle>
            </CardHeader>
            <CardContent>
              <StrategyConfigForm
                type={type}
                config={config}
                onChange={setConfig}
              />
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Position Limits</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <NumberInput
                label="Max Positions"
                hint="Maximum number of simultaneous positions"
                value={config.maxPositions}
                onChange={(v) => setConfig({ ...config, maxPositions: v })}
                min={1}
                step={1}
              />
              <NumberInput
                label="Max Position Percent"
                hint="Max budget allocation per position (0-1)"
                value={config.maxPositionPercent}
                onChange={(v) =>
                  setConfig({ ...config, maxPositionPercent: v })
                }
                min={0.01}
                max={1}
                step={0.01}
              />
              <NumberInput
                label="Hold Period Days"
                hint="Maximum days to hold a position"
                value={config.holdPeriodDays}
                onChange={(v) => setConfig({ ...config, holdPeriodDays: v })}
                min={1}
                step={1}
              />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
