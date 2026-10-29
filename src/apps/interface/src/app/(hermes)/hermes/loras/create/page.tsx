"use client";

import { Typography } from "@/components/shared/typography";
import { LoraForm } from "@/app/(hermes)/hermes/components/lora-form";
import { type LoraFormInput } from "@/app/(hermes)/hermes/types";
import { hermesApi } from "@/lib/api/hermes";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function CreateLoraPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);

  const handleSave = async (input: LoraFormInput) => {
    setLoading(true);
    try {
      await hermesApi.createLora({
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

  return (
    <div className="m-4">
      <Typography variant="h2" className="border-b-0">
        Register LoRA
      </Typography>

      <LoraForm onSave={handleSave} loading={loading} />
    </div>
  );
}
