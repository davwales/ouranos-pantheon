"use client";

import { Typography } from "@/components/shared/typography";
import { TraitForm } from "@/app/(hermes)/hermes/components/trait-form";
import { TraitFormInput } from "@/app/(hermes)/hermes/types";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { FormSkeleton } from "@/components/shared/skeletons/form-skeleton";
import { NotFoundCard } from "@/components/shared/not-found-card";
import { usePublicMode } from "@/app/(hermes)/hermes/components/public-mode-context";

export default function EditTraitPage() {
  const isPublicMode = usePublicMode();
  const router = useRouter();
  const { traitId } = useParams<{ traitId: string }>();
  const [loading, setLoading] = useState(false);

  const [state] = useApi(() => hermesApi.getTrait(traitId), [traitId]);
  useBreadcrumbLabel(traitId, state.data?.name, state.status === "error");

  const fetching = state.status === "loading";

  const trait: TraitFormInput | undefined = state.data;

  if (state.status === "error" && !state.data) {
    return <NotFoundCard title="Trait not found" backHref="/hermes/traits" backLabel="Back to Traits" />;
  }

  const handleDelete = async () => {
    setLoading(true);
    try {
      await hermesApi.deleteTrait(traitId);
      router.push("/hermes/traits");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (input: TraitFormInput) => {
    setLoading(true);
    try {
      await hermesApi.updateTrait({
        traitId: traitId,
        name: input.name,
        content: input.content,
        isPublic: input.isPublic,
      });
      router.push("/hermes/traits");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (fetching || !trait) {
    return (
      <div className="m-4">
        <FormSkeleton fields={2} hasTitle checkboxes={1} textareas={1} />
      </div>
    );
  }

  return (
    <div className="m-4">
      <Typography variant="h2" className="border-b-0">
        Edit Trait
      </Typography>

      <TraitForm
        initial={trait}
        onSave={isPublicMode ? undefined : handleSave}
        onDelete={isPublicMode ? undefined : handleDelete}
        loading={loading || fetching}
        className="mt-4"
      />
    </div>
  );
}
