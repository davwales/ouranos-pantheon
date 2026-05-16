"use client";

import { Typography } from "@/components/shared/typography";
import { TraitForm } from "@/app/(hermes)/hermes/components/trait-form";
import { TraitFormInput } from "@/app/(hermes)/hermes/types";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { FormSkeleton } from "@/components/shared/skeletons/form-skeleton";
import { NotFoundCard } from "@/components/shared/not-found-card";

export default function EditTraitPage() {
  const router = useRouter();
  const { traitId } = useParams<{ traitId: string }>();
  const [loading, setLoading] = useState(false);
  const [trait, setTrait] = useState<TraitFormInput>();

  const [state] = useApi(() => hermesApi.getTrait(traitId), [traitId]);

  const fetching = state.status === "loading";

  useEffect(() => {
    if (state.status === "success") {
      setTrait({ ...state.data });
    }
  }, [state]);

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
        onSave={handleSave}
        onDelete={handleDelete}
        loading={loading || fetching}
        className="mt-4"
      />
    </div>
  );
}
