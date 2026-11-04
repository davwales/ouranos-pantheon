"use client";

import { Typography } from "@/components/shared/typography";
import { ModelForm } from "@/app/(hermes)/hermes/components/model-form";
import { ModelFormInput } from "@/app/(hermes)/hermes/types";
import { Badge } from "@/components/ui/badge";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { AlertTriangle } from "lucide-react";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { FormSkeleton } from "@/components/shared/skeletons/form-skeleton";
import { NotFoundCard } from "@/components/shared/not-found-card";

export default function EditModelPage() {
  const router = useRouter();
  const { modelId } = useParams<{ modelId: string }>();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [state] = useApi(() => hermesApi.getModel(modelId), [modelId]);
  useBreadcrumbLabel(modelId, state.data?.name, state.status === "error");

  const fetching = state.status === "loading";

  const model: ModelFormInput | undefined = state.data;

  if (state.status === "error" && !state.data) {
    return <NotFoundCard title="Model not found" backHref="/hermes/models" backLabel="Back to Models" />;
  }

  const handleDelete = async () => {
    setLoading(true);
    setError(null);
    try {
      await hermesApi.deleteModel(modelId);
      router.push("/hermes/models");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete model");
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (input: ModelFormInput) => {
    setLoading(true);
    setError(null);
    try {
      await hermesApi.updateModel({
        modelId: modelId,
        name: input.name,
        modelIdentifier: input.modelIdentifier,
        systemPrompt: input.systemPrompt,
        temperature: input.temperature,
        maxTokens: input.maxTokens,
        defaultToolSetId: input.defaultToolSetId,
        contextWindow: input.contextWindow,
        isDefault: input.isDefault,
        isPublic: input.isPublic,
      });
      router.push("/hermes/models");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save model");
    } finally {
      setLoading(false);
    }
  };

  if (fetching || !model) {
    return (
      <div className="m-4">
        <FormSkeleton fields={7} hasTitle checkboxes={2} />
      </div>
    );
  }

  return (
    <div className="m-4">
      <div className="flex items-center gap-3">
        <Typography variant="h2" className="border-b-0">
          Edit Model
        </Typography>
        {model.isUnavailable && (
          <Badge variant="destructive" className="gap-1">
            <AlertTriangle className="h-3 w-3" />
            Unavailable
          </Badge>
        )}
      </div>

      {error && (
        <div className="mt-4 rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
          {error}
        </div>
      )}

      <ModelForm
        initial={model}
        onSave={handleSave}
        onDelete={handleDelete}
        loading={loading || fetching}
        className="mt-4"
      />
    </div>
  );
}
