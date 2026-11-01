"use client";

import {
  ToolSetForm,
  toSaveToolSetInput,
  toToolSetFormInput,
} from "@/app/(hermes)/hermes/components/tool-set-form";
import { type ToolSetFormInput } from "@/app/(hermes)/hermes/types";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { NotFoundCard } from "@/components/shared/not-found-card";
import { FormSkeleton } from "@/components/shared/skeletons/form-skeleton";
import { Typography } from "@/components/shared/typography";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { usePublicMode } from "@/app/(hermes)/hermes/components/public-mode-context";

export default function EditToolSetPage() {
  const isPublicMode = usePublicMode();
  const router = useRouter();
  const { toolSetId } = useParams<{ toolSetId: string }>();
  const [loading, setLoading] = useState(false);

  const [state] = useApi(() => hermesApi.getToolSet(toolSetId), [toolSetId]);
  const [toolsState] = useApi(() => hermesApi.getAvailableTools());
  useBreadcrumbLabel(toolSetId, state.data?.name, state.status === "error");

  if (state.status === "error" && !state.data) {
    return (
      <NotFoundCard
        title="Tool set not found"
        backHref="/hermes/tool-sets"
        backLabel="Back to Tool Sets"
      />
    );
  }

  const handleDelete = async () => {
    setLoading(true);
    try {
      await hermesApi.deleteToolSet(toolSetId);
      router.push("/hermes/tool-sets");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (input: ToolSetFormInput) => {
    setLoading(true);
    try {
      await hermesApi.updateToolSet(toolSetId, toSaveToolSetInput(input));
      router.push("/hermes/tool-sets");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (!state.data || (toolsState.status === "loading" && !toolsState.data)) {
    return (
      <div className="m-4">
        <FormSkeleton fields={3} hasTitle checkboxes={1} />
      </div>
    );
  }

  return (
    <div className="m-4">
      <Typography variant="h2" className="border-b-0">
        Edit Tool Set
      </Typography>

      <ToolSetForm
        key={state.data.id}
        initial={toToolSetFormInput(state.data)}
        availableTools={toolsState.data ?? []}
        onSave={isPublicMode ? undefined : handleSave}
        onDelete={isPublicMode ? undefined : handleDelete}
        loading={loading}
        className="mt-4"
      />
    </div>
  );
}
