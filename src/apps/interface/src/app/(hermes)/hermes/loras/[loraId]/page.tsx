"use client";

import { Typography } from "@/components/shared/typography";
import { LoraForm } from "@/app/(hermes)/hermes/components/lora-form";
import { LoraStatusBadge } from "@/app/(hermes)/hermes/components/lora-status-badge";
import { type LoraFormInput } from "@/app/(hermes)/hermes/types";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { FormSkeleton } from "@/components/shared/skeletons/form-skeleton";
import { NotFoundCard } from "@/components/shared/not-found-card";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";

export default function EditLoraPage() {
  const router = useRouter();
  const { loraId } = useParams<{ loraId: string }>();
  const [loading, setLoading] = useState(false);

  const [state] = useApi(() => hermesApi.getLora(loraId), [loraId]);
  useBreadcrumbLabel(loraId, state.data?.name, state.status === "error");

  if (state.status === "error" && !state.data) {
    return (
      <NotFoundCard
        title="LoRA not found"
        backHref="/hermes/loras"
        backLabel="Back to LoRAs"
      />
    );
  }

  const handleDelete = async () => {
    setLoading(true);
    try {
      await hermesApi.deleteLora(loraId);
      router.push("/hermes/loras");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (input: LoraFormInput) => {
    setLoading(true);
    try {
      await hermesApi.updateLora(loraId, {
        name: input.name,
        identifier: input.identifier,
        weight: input.weight,
        isPublic: input.isPublic,
        triggerPhrase: input.triggerPhrase?.trim() || null,
      });
      router.push("/hermes/loras");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (state.status === "loading" || !state.data) {
    return (
      <div className="m-4">
        <FormSkeleton fields={4} hasTitle checkboxes={1} />
      </div>
    );
  }

  return (
    <div className="m-4">
      <div className="flex items-center gap-3">
        <Typography variant="h2" className="border-b-0">
          Edit LoRA
        </Typography>
        <LoraStatusBadge lora={state.data} />
      </div>

      <LoraForm
        initial={state.data}
        onSave={handleSave}
        onDelete={handleDelete}
        loading={loading}
        className="mt-4"
      />
    </div>
  );
}
