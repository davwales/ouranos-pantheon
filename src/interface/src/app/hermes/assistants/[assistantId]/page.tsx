"use client";

import { Typography } from "@/app/components/typography";
import { AssistantForm } from "@/app/hermes/components/assistant_form";
import { DELETE_ASSISTANT, UPDATE_ASSISTANT } from "@/app/hermes/mutations";
import { GET_ASSISTANT } from "@/app/hermes/queries";
import { AssistantFormInput } from "@/app/hermes/types";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useMutation, useQuery } from "urql";

export default function EditAssistantPage() {
  const router = useRouter();
  const { assistantId: assistantId } = useParams<{ assistantId: string }>();
  const [loading, setLoading] = useState(false);
  const [assistant, setAssistant] = useState<AssistantFormInput>();

  const [{ data, fetching, error: fetchError }] = useQuery({
    query: GET_ASSISTANT,
    variables: { assistantId: assistantId },
  });

  useEffect(() => {
    if (data?.assistant) {
      setAssistant({ ...data.assistant });
    }
  }, [data, fetchError]);

  const [, updateAssistant] = useMutation(UPDATE_ASSISTANT);
  const [, deleteAssistant] = useMutation(DELETE_ASSISTANT);

  const handleDelete = () => {
    setLoading(true);

    try {
      deleteAssistant({ input: { assistantId: assistantId } });
      setLoading(false);
      router.push("/hermes/assistants");
    } catch (err: any) {
      setLoading(false);
    }
  };

  const handleSave = async (input: AssistantFormInput) => {
    setLoading(true);

    try {
      await updateAssistant({
        input: {
          assistantId: assistantId,
          model: input.model,
          systemPrompt: input.systemPrompt,
          assistantName: input.assistantName,
          userName: input.userName,
          temperature: input.temperature,
          maxTokens: input.maxTokens,
          repeatPenalty: input.repeatPenalty,
        },
      });

      setLoading(false);
      router.push("/hermes/assistants");
    } catch (err: any) {
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
