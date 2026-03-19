import InfoCard from "@/app/components/info-card";
import { AssistantForm } from "@/app/hermes/components/assistant_form";
import ConversationAssistant from "@/app/hermes/types";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { useApi } from "@/hooks/use-api";
import { hermesApi, Role } from "@/lib/api/hermes";
import { useState } from "react";

export default function SelectAssistantView({
  role,
  assistant,
  setAssistant,
  className,
  ...props
}: React.ComponentProps<"div"> & {
  role: Role;
  assistant?: ConversationAssistant | undefined;
  setAssistant: (assistant: ConversationAssistant | undefined) => void;
}) {
  const [drawerOpen, setDrawerOpen] = useState<boolean>(false);

  const [state] = useApi(() => hermesApi.getAllAssistants(), []);

  const handleAssistantModified = (
    modifiedAssistant: ConversationAssistant,
  ) => {
    setDrawerOpen(false);
    setAssistant(modifiedAssistant);
  };

  const handleAssistantSelected = (
    selectedAssistant: ConversationAssistant,
  ) => {
    if (selectedAssistant.id == assistant?.id) {
      setAssistant(undefined);
      return;
    }

    setAssistant(selectedAssistant);
  };

  return (
    <div {...props} className={`mt-4 ${className}`}>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {state.data?.map((a) => (
          <InfoCard
            key={a.id}
            label={a.assistantName}
            description={`Model: ${a.model}`}
            onClick={() => handleAssistantSelected(a)}
            className={`hover:bg-accent hover:cursor-pointer h-full w-full ${
              a.id == assistant?.id ? "border-accent-foreground" : ""
            }`}
          />
        ))}
      </div>

      {assistant && (
        <Drawer open={drawerOpen} onOpenChange={setDrawerOpen}>
          <DrawerTrigger className="w-full mt-4 p-2 rounded hover:cursor-pointer hover:bg-accent/50">
            Modify Assistant
          </DrawerTrigger>
          <DrawerContent>
            <DrawerHeader>
              <DrawerTitle>Modifying {assistant.assistantName}</DrawerTitle>
              <DrawerDescription>
                Make slight adjustments to {assistant.assistantName} for this
                specific conversation.
              </DrawerDescription>
            </DrawerHeader>
            <AssistantForm
              initial={assistant}
              onSave={handleAssistantModified}
              className="mx-4 px-2 pb-4 rounded overflow-scroll"
            />
          </DrawerContent>
        </Drawer>
      )}
    </div>
  );
}
