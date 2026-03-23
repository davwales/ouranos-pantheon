"use client";

import { Typography } from "@/app/components/typography";
import { ModelForm } from "@/app/hermes/components/model_form";
import { ModelFormInput } from "@/app/hermes/types";
import { hermesApi } from "@/lib/api/hermes";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function CreateModelPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);

  const handleSave = async (input: ModelFormInput) => {
    setLoading(true);
    try {
      await hermesApi.createModel({
        name: input.name,
        modelIdentifier: input.modelIdentifier,
        systemPrompt: input.systemPrompt,
        temperature: input.temperature,
        maxTokens: input.maxTokens,
        repeatPenalty: input.repeatPenalty,
        isDefault: input.isDefault,
      });
      router.push("/hermes/models");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="m-4">
      <Typography variant="h2" className="border-b-0">
        Create Model
      </Typography>

      <ModelForm onSave={handleSave} loading={loading} />
    </div>
  );
}
