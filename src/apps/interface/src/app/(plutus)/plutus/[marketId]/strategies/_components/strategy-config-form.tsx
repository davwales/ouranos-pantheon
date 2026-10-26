"use client";

import { Button } from "@/components/ui/button";
import {
  type FlipConfiguration,
  type StrategyConfiguration,
  type StrategyDetail,
  type StrategyMode,
  type SwingConfiguration,
  plutusApi,
} from "@/lib/api/plutus";
import { RefreshCw } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { DEFAULT_FLIP_CONFIGURATION } from "./flip-constants";
import { FlipSettingsCard } from "./flip-settings-card";
import { withValidLocks } from "./parameter-locks";
import { StrategyBasicInfoCard } from "./strategy-basic-info-card";
import { StrategyModeCard } from "./strategy-mode-card";
import { SwingConfigForm } from "./swing-config-form";
import {
  DEFAULT_SWING_CONFIGURATION,
  withAllInputKinds,
  withoutZeroWeights,
} from "./swing-constants";
import { useUnsavedChangesGuard } from "@/hooks/use-unsaved-changes-guard";
import { hasChanges } from "@/lib/has-changes";

type ConfigurationDrafts = {
  Swing: SwingConfiguration;
  Flip: FlipConfiguration;
};

function buildInitialDrafts(
  configuration?: StrategyConfiguration,
): ConfigurationDrafts {
  const drafts: ConfigurationDrafts = {
    Swing: DEFAULT_SWING_CONFIGURATION,
    Flip: DEFAULT_FLIP_CONFIGURATION,
  };
  if (configuration?.mode === "Swing") {
    drafts.Swing = withAllInputKinds(configuration);
  } else if (configuration?.mode === "Flip") {
    drafts.Flip = configuration;
  }
  return drafts;
}

function toRequestConfiguration(
  configuration: StrategyConfiguration,
): StrategyConfiguration {
  return withValidLocks(
    configuration.mode === "Swing"
      ? withoutZeroWeights(configuration)
      : configuration,
  );
}

function validateConfiguration(
  configuration: StrategyConfiguration,
): string | null {
  if (
    configuration.mode === "Swing" &&
    configuration.inputWeights.length === 0
  ) {
    return "At least one input weight must be non-zero";
  }
  return null;
}

export function StrategyConfigForm({
  marketId,
  mode,
  strategyId,
  initialStrategy,
  onSuccess,
  onCancel,
}: {
  marketId: string;
  mode: "create" | "edit";
  strategyId?: string;
  initialStrategy?: StrategyDetail;
  onSuccess?: () => void;
  onCancel?: () => void;
}) {
  const router = useRouter();
  const isEdit = mode === "edit";

  const [name, setName] = useState(initialStrategy?.name ?? "");
  const [description, setDescription] = useState(
    initialStrategy?.description ?? "",
  );
  const [strategyMode, setStrategyMode] = useState<StrategyMode>(
    initialStrategy?.configuration.mode ?? "Swing",
  );
  const [drafts, setDrafts] = useState<ConfigurationDrafts>(() =>
    buildInitialDrafts(initialStrategy?.configuration),
  );
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const editable = { name, description, strategyMode, drafts };
  const [baseline] = useState(editable);
  useUnsavedChangesGuard(hasChanges(editable, baseline));

  const handleModeChange = (next: StrategyMode) => {
    setStrategyMode(next);
    setError(null);
  };

  const handleSwingChange = (swing: SwingConfiguration) => {
    setDrafts((current) => ({ ...current, Swing: swing }));
    setError(null);
  };

  const handleFlipChange = (flip: FlipConfiguration) => {
    setDrafts((current) => ({ ...current, Flip: flip }));
    setError(null);
  };

  const handleSubmit = async (e?: React.FormEvent) => {
    e?.preventDefault();
    setIsProcessing(true);
    setError(null);
    try {
      const configuration = toRequestConfiguration(drafts[strategyMode]);
      const validationError = validateConfiguration(configuration);
      if (validationError) {
        setError(validationError);
        return;
      }

      if (isEdit && strategyId) {
        await plutusApi.updateStrategy(strategyId, {
          name: name.trim(),
          description: description.trim() || null,
          configuration,
        });
        onSuccess?.();
      } else {
        const response = await plutusApi.createStrategy({
          marketId,
          name: name.trim(),
          description: description.trim() || null,
          configuration,
        });
        router.replace(`/plutus/${marketId}/strategies/${response.id}`);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save strategy");
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <form className="space-y-6" onSubmit={handleSubmit}>
      {error && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
          {error}
        </div>
      )}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <StrategyBasicInfoCard
            name={name}
            description={description}
            onNameChange={setName}
            onDescriptionChange={setDescription}
          />
          {strategyMode === "Swing" ? (
            <SwingConfigForm
              configuration={drafts.Swing}
              onChange={handleSwingChange}
            />
          ) : (
            <FlipSettingsCard
              configuration={drafts.Flip}
              onChange={handleFlipChange}
            />
          )}
        </div>

        <div className="space-y-6">
          <StrategyModeCard mode={strategyMode} onChange={handleModeChange} />
        </div>
      </div>

      <div className="flex items-center justify-end gap-3">
        {isProcessing && <RefreshCw className="animate-spin size-4" />}
        {onCancel && (
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            disabled={isProcessing}
          >
            Cancel
          </Button>
        )}
        <Button type="submit" disabled={isProcessing || !name.trim()}>
          {isProcessing
            ? isEdit
              ? "Saving..."
              : "Creating..."
            : isEdit
              ? "Save"
              : "Create"}
        </Button>
      </div>
    </form>
  );
}
