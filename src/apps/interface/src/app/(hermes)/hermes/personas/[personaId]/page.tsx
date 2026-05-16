"use client";

import { Typography } from "@/components/shared/typography";
import { PersonaForm } from "@/app/(hermes)/hermes/components/persona-form";
import { PersonaFormInput } from "@/app/(hermes)/hermes/types";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { FormSkeleton } from "@/components/shared/skeletons/form-skeleton";
import { NotFoundCard } from "@/components/shared/not-found-card";

export default function EditPersonaPage() {
  const router = useRouter();
  const { personaId } = useParams<{ personaId: string }>();
  const [loading, setLoading] = useState(false);
  const [persona, setPersona] = useState<PersonaFormInput>();

  const [state] = useApi(() => hermesApi.getPersona(personaId), [personaId]);

  if (state.status === "error" && !state.data) {
    return <NotFoundCard title="Persona not found" backHref="/hermes/personas" backLabel="Back to Personas" />;
  }

  const fetching = state.status === "loading";

  useEffect(() => {
    if (state.status === "success") {
      setPersona({ ...state.data });
    }
  }, [state]);

  const handleDelete = async () => {
    setLoading(true);
    try {
      await hermesApi.deletePersona(personaId);
      router.push("/hermes/personas");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (input: PersonaFormInput) => {
    setLoading(true);
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
      console.error(err);
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
