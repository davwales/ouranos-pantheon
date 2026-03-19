"use client";

import ChatInterfaceView from "@/app/hermes/conversation/views/chat_interface_view";
import SelectAssistantView from "@/app/hermes/conversation/views/select_assistant_view";
import ConversationAssistant from "@/app/hermes/types";
import { Button } from "@/components/ui/button";
import { Role } from "@/lib/api/hermes";
import { useState } from "react";

export default function Conversation() {
  const [assistant, setAssistant] = useState<ConversationAssistant>();
  const [setupComplete, setSetupComplete] = useState(false);

  const handleBeginConversation = () => {
    if (assistant) {
      setSetupComplete(true);
    } else {
      alert("Please select an assistant before starting the conversation.");
    }
  };

  return (
    <div>
      {setupComplete && assistant ? (
        <ChatInterfaceView assistant={assistant} />
      ) : (
        <div className="m-4">
          <SelectAssistantView
            role={Role.Assistant}
            assistant={assistant}
            setAssistant={setAssistant}
          />

          <Button
            onClick={handleBeginConversation}
            disabled={!assistant}
            size="lg"
            className="w-full mt-4"
          >
            Begin Conversation
          </Button>
        </div>
      )}
    </div>
  );
}
