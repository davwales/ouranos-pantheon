"use client";

import { Typography } from "@/components/shared/typography";
import { PersonaForm } from "@/app/(hermes)/hermes/components/persona-form";
import { PersonaFormInput } from "@/app/(hermes)/hermes/types";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { FormSkeleton } from "@/components/shared/skeletons/form-skeleton";
import { NotFoundCard } from "@/components/shared/not-found-card";

export default function EditPersonaPage() {
  const router = useRouter();
  const { personaId } = useParams<{ personaId: string }>();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [state] = useApi(() => hermesApi.getPersona(personaId), [personaId]);
  useBreadcrumbLabel(personaId, state.data?.name, state.status === "error");

  const fetching = state.status === "loading";

  const persona: PersonaFormInput | undefined = state.data;

  if (state.status === "error" && !state.data) {
    return <NotFoundCard title="Persona not found" backHref="/hermes/personas" backLabel="Back to Personas" />;
  }

  const handleDelete = async () => {
    setLoading(true);
    setError(null);
    try {
      await hermesApi.deletePersona(personaId);
      router.push("/hermes/personas");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete persona");
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (input: PersonaFormInput) => {
    setLoading(true);
    setError(null);
    try {
      await hermesApi.updatePersona({
        personaId: personaId,
        name: input.name,
        description: input.description,
        personality: input.personality,
        scenario: input.scenario,
        isDefault: input.isDefault,
        isPublic: input.isPublic,
      });
      router.push("/hermes/personas");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save persona");
    } finally {
      setLoading(false);
    }
  };

  if (fetching || !persona) {
    return (
      <div className="m-4">
        <FormSkeleton fields={4} hasTitle checkboxes={2} />
      </div>
    );
  }

  return (
    <div className="m-4">
      <Typography variant="h2" className="border-b-0">
        Edit Persona
      </Typography>

      {error && (
        <div className="mt-4 rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
          {error}
        </div>
      )}

      <PersonaForm
        initial={persona}
        onSave={handleSave}
        onDelete={handleDelete}
        loading={loading || fetching}
        className="mt-4"
      />
    </div>
  );
}
