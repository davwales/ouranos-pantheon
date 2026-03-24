"use client";

import { Typography } from "@/app/components/typography";
import { TraitForm } from "@/app/hermes/components/trait_form";
import { TraitFormInput } from "@/app/hermes/types";
import { hermesApi } from "@/lib/api/hermes";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function CreateTraitPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);

  const handleSave = async (input: TraitFormInput) => {
    setLoading(true);
    try {
      await hermesApi.createTrait({
        name: input.name,
        content: input.content,
      });
      router.push("/hermes/traits");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="m-4">
      <Typography variant="h2" className="border-b-0">
        Create Trait
      </Typography>

      <TraitForm onSave={handleSave} loading={loading} />
    </div>
  );
}
