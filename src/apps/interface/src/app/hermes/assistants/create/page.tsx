"use client";

import { Typography } from "@/app/components/typography";
import { AssistantForm } from "@/app/hermes/components/assistant_form";
import { AssistantFormInput } from "@/app/hermes/types";
import { hermesApi } from "@/lib/api/hermes";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function CreateAssistantPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);

  const handleSave = async (input: AssistantFormInput) => {
    setLoading(true);
    try {
      await hermesApi.createAssistant({
        model: input.model,
        systemPrompt: input.systemPrompt,
        assistantName: input.assistantName,
        userName: input.userName,
        temperature: input.temperature,
        maxTokens: input.maxTokens,
        repeatPenalty: input.repeatPenalty,
      });
      router.push("/hermes/assistants");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="m-4">
      <Typography variant="h2" className="border-b-0">
        Create Assistant
      </Typography>

      <AssistantForm onSave={handleSave} loading={loading} />
    </div>
  );
}
