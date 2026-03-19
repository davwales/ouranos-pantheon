"use client";

import { Typography } from "@/app/components/typography";
import { AssistantForm } from "@/app/hermes/components/assistant_form";
import { AssistantFormInput } from "@/app/hermes/types";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";

export default function EditAssistantPage() {
  const router = useRouter();
  const { assistantId } = useParams<{ assistantId: string }>();
  const [loading, setLoading] = useState(false);
  const [assistant, setAssistant] = useState<AssistantFormInput>();

  const [state] = useApi(
    () => hermesApi.getAssistant(assistantId),
    [assistantId],
  );

  const fetching = state.status === "loading";

  useEffect(() => {
    if (state.status === "success") {
      setAssistant({ ...state.data });
    }
  }, [state]);

  const handleDelete = async () => {
    setLoading(true);
    try {
      await hermesApi.deleteAssistant(assistantId);
      router.push("/hermes/assistants");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (input: AssistantFormInput) => {
    setLoading(true);
    try {
      await hermesApi.updateAssistant({
        assistantId: assistantId,
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

  if (fetching || !assistant) {
    return <div>Loading...</div>;
  }

  return (
    <div className="m-4">
      <Typography variant="h2" className="border-b-0">
        Edit Assistant
      </Typography>

      <AssistantForm
        initial={assistant}
        onSave={handleSave}
        onDelete={handleDelete}
        loading={loading || fetching}
        className="mt-4"
      />
    </div>
  );
}
