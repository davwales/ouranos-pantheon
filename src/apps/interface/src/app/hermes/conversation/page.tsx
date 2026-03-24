"use client";

import ChatInterfaceView from "@/app/hermes/conversation/views/chat_interface_view";
import SelectConfigView from "@/app/hermes/conversation/views/select_config_view";
import { ModelFormInput, PersonaFormInput, TraitFormInput } from "@/app/hermes/types";
import { hermesApi } from "@/lib/api/hermes";
import { useEffect, useState } from "react";

type ConversationState = "loading" | "setup" | "chat";

export default function Conversation() {
  const [persona, setPersona] = useState<PersonaFormInput>();
  const [model, setModel] = useState<ModelFormInput>();
  const [activeTraits, setActiveTraits] = useState<TraitFormInput[]>([]);
  const [conversationState, setConversationState] =
    useState<ConversationState>("loading");

  useEffect(() => {
    const loadDefaults = async () => {
      try {
        const [personas, models] = await Promise.all([
          hermesApi.getAllPersonas(),
          hermesApi.getAllModels(),
        ]);

        const defaultPersona = personas.find((p) => p.isDefault);
        const defaultModel = models.find((m) => m.isDefault);

        if (defaultPersona) {
          setPersona({ ...defaultPersona });
        }
        if (defaultModel) {
          setModel({ ...defaultModel });
        }

        if (defaultPersona && defaultModel) {
          setConversationState("chat");
        } else {
          setConversationState("setup");
        }
      } catch {
        setConversationState("setup");
      }
    };

    loadDefaults();
  }, []);

  const handleBeginConversation = () => {
    if (persona && model) {
      setConversationState("chat");
    }
  };

  if (conversationState === "loading") {
    return <div className="m-4">Loading...</div>;
  }

  if (conversationState === "chat" && persona && model) {
    return (
      <ChatInterfaceView
        persona={persona}
        model={model}
        activeTraits={activeTraits}
        onPersonaChange={setPersona}
        onModelChange={setModel}
        onTraitsChange={setActiveTraits}
      />
    );
  }

  return (
    <div className="m-4">
      <SelectConfigView
        persona={persona}
        setPersona={setPersona}
        model={model}
        setModel={setModel}
        onBegin={handleBeginConversation}
        beginDisabled={!persona || !model}
      />
    </div>
  );
}
