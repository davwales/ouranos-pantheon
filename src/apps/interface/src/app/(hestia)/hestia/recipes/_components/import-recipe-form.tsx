"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { hestiaApi } from "@/lib/api/hestia";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { isValidRecipeUrl } from "./validate-recipe-url";

export type ImportRecipeFormProps = {
  onDone: () => void;
};

export function ImportRecipeForm({ onDone }: ImportRecipeFormProps) {
  const router = useRouter();
  const [url, setUrl] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const urlInvalid = url.trim() !== "" && !isValidRecipeUrl(url);

  const handleSubmit = async () => {
    setIsSubmitting(true);
    setError(null);
    try {
      const { id } = await hestiaApi.importRecipe({ url: url.trim() });
      onDone();
      router.push(`/hestia/recipes/${id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to import recipe");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-4">
      {error && (
        <div
          role="alert"
          className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive"
        >
          {error}
        </div>
      )}
      <div className="space-y-1">
        <label htmlFor="recipe-url" className="text-sm font-medium block">
          Recipe URL
        </label>
        <Input
          id="recipe-url"
          type="url"
          placeholder="https://example.com/recipe"
          value={url}
          onChange={(e) => setUrl(e.target.value)}
        />
      </div>
      {urlInvalid && (
        <p className="text-sm text-destructive">Enter a valid http(s) URL</p>
      )}
      <Button
        className="w-full"
        onClick={handleSubmit}
        disabled={isSubmitting || !isValidRecipeUrl(url)}
      >
        {isSubmitting ? "Importing..." : "Import from Link"}
      </Button>
    </div>
  );
}
