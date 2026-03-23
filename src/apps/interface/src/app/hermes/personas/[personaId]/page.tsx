"use client";

import { Typography } from "@/app/components/typography";
import { PersonaForm } from "@/app/hermes/components/persona_form";
import { PersonaFormInput } from "@/app/hermes/types";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";

export default function EditPersonaPage() {
  const router = useRouter();
  const { personaId } = useParams<{ personaId: string }>();
  const [loading, setLoading] = useState(false);
  const [persona, setPersona] = useState<PersonaFormInput>();

  const [state] = useApi(() => hermesApi.getPersona(personaId), [personaId]);

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
      });
      router.push("/hermes/personas");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (fetching || !persona) {
    return <div>Loading...</div>;
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
