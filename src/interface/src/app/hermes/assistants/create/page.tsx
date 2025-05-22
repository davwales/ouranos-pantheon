"use client";

import { Typography } from "@/app/components/typography";
import { AssistantForm } from "@/app/hermes/components/assistant_form";
import { CREATE_ASSISTANT } from "@/app/hermes/mutations";
import { AssistantFormInput } from "@/app/hermes/types";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useMutation } from "urql";

export default function CreateAssistantPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);

  const [, createAssistant] = useMutation(CREATE_ASSISTANT);

  const handleSave = async (input: AssistantFormInput) => {
    setLoading(true);

    try {
      await createAssistant({
        input: { ...input },
      });
      setLoading(false);
      router.push("/hermes/assistants");
    } catch (err: any) {
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
