"use client";

import {
  ToolSetForm,
  toSaveToolSetInput,
} from "@/app/(hermes)/hermes/components/tool-set-form";
import { type ToolSetFormInput } from "@/app/(hermes)/hermes/types";
import { FormSkeleton } from "@/components/shared/skeletons/form-skeleton";
import { Typography } from "@/components/shared/typography";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function CreateToolSetPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [toolsState] = useApi(() => hermesApi.getAvailableTools());

  const handleSave = async (input: ToolSetFormInput) => {
    setLoading(true);
    try {
      await hermesApi.createToolSet(toSaveToolSetInput(input));
      router.push("/hermes/tool-sets");
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="m-4">
      <Typography variant="h2" className="border-b-0">
        Create Tool Set
      </Typography>

      {toolsState.status === "loading" && !toolsState.data ? (
        <FormSkeleton fields={3} checkboxes={1} />
      ) : (
        <ToolSetForm
          availableTools={toolsState.data ?? []}
          onSave={handleSave}
          loading={loading}
        />
      )}
    </div>
  );
}
