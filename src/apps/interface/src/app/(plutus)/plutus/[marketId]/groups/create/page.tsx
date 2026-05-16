"use client";

import { Typography } from "@/components/shared/typography";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { plutusApi } from "@/lib/api/plutus";
import { RefreshCw } from "lucide-react";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";

export default function CreateGroupPage() {
  const { marketId } = useParams<{ marketId: string }>();
  const router = useRouter();
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [isProcessing, setIsProcessing] = useState(false);

  const handleSubmit = async () => {
    setIsProcessing(true);
    try {
      const response = await plutusApi.createSymbolGroup({
        marketId,
        name,
        description: description || null,
      });
      router.replace(`/plutus/${marketId}/groups/${response.id}`);
    } catch (error) {
      console.error("Failed to create group:", error);
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between">
        <Typography variant="h2" className="border-b-0">
          Create New Group
        </Typography>
        <div className="flex items-center gap-2">
          {isProcessing && <RefreshCw className="animate-spin" />}
          <Button
            onClick={handleSubmit}
            disabled={isProcessing || !name.trim()}
          >
            Save
          </Button>
        </div>
      </div>

      <div className="mt-4 space-y-4 max-w-lg">
        <div className="space-y-1">
          <label htmlFor="name" className="text-sm font-medium">
            Name
          </label>
          <Input
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Potions, Wishlist"
          />
        </div>
        <div className="space-y-1">
          <label htmlFor="description" className="text-sm font-medium">
            Description (optional)
          </label>
          <Textarea
            id="description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="What is this group for?"
            rows={3}
          />
        </div>
      </div>
    </div>
  );
}
