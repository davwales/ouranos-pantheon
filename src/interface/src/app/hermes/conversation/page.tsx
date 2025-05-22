"use client";

import { Step } from "@/app/components/stepper";
import Stepper from "@/app/components/stepper/stepper";
import ChatInterfaceView from "@/app/hermes/conversation/views/chat_interface_view";
import SelectAssistantView from "@/app/hermes/conversation/views/select_assistant_view";
import ConversationAssistant from "@/app/hermes/types";
import { Role } from "@/gql/graphql";
import { useState } from "react";

export default function Conversation() {
  const [assistant, setAssistant] = useState<ConversationAssistant>();
  const [setupComplete, setSetupComplete] = useState(false);

  const steps: Step[] = [
    {
      label: "Assistant",
      content: (
        <SelectAssistantView
          role={Role.Assistant}
          assistant={assistant}
          setAssistant={setAssistant}
        />
      ),
    },
  ];

  const handleStepperComplete = () => {
    setSetupComplete(true);
  };

  const ChatDisplay = () =>
    assistant ? (
      <ChatInterfaceView assistant={assistant} />
    ) : (
      "Invalid conversation configuration. Please refresh and try again."
    );

  return (
    <div>
      {setupComplete ? (
        <ChatDisplay />
      ) : (
        <Stepper
          steps={steps}
          onComplete={handleStepperComplete}
          className="m-4"
        />
      )}
    </div>
  );
}
