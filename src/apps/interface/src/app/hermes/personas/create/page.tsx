"use client";

import { Typography } from "@/app/components/typography";
import { PersonaForm } from "@/app/hermes/components/persona_form";
import { PersonaFormInput } from "@/app/hermes/types";
import { hermesApi } from "@/lib/api/hermes";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function CreatePersonaPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);

  const handleSave = async (input: PersonaFormInput) => {
    setLoading(true);
    try {
      await hermesApi.createPersona({
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

  return (
    <div className="m-4">
      <Typography variant="h2" className="border-b-0">
        Create Persona
      </Typography>

      <PersonaForm onSave={handleSave} loading={loading} />
    </div>
  );
}
