import InfoCard from "@/app/components/info-card";
import { ModelFormInput, PersonaFormInput } from "@/app/hermes/types";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";

export default function SelectConfigView({
  persona,
  setPersona,
  model,
  setModel,
  onBegin,
  beginDisabled,
}: {
  persona?: PersonaFormInput;
  setPersona: (persona: PersonaFormInput | undefined) => void;
  model?: ModelFormInput;
  setModel: (model: ModelFormInput | undefined) => void;
  onBegin: () => void;
  beginDisabled: boolean;
}) {
  const [personasState] = useApi(() => hermesApi.getAllPersonas());
  const [modelsState] = useApi(() => hermesApi.getAllModels());

  const availableModels = modelsState.data?.filter((m) => !m.isUnavailable) ?? [];

  const handlePersonaSelected = (selected: PersonaFormInput) => {
    if (selected.id === persona?.id) {
      setPersona(undefined);
    } else {
      setPersona(selected);
    }
  };

  const handleModelSelected = (selected: ModelFormInput) => {
    if (selected.id === model?.id) {
      setModel(undefined);
    } else {
      setModel(selected);
    }
  };

  return (
    <div>
      <div className="mt-4">
        <p className="text-sm font-medium mb-2">Persona</p>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {personasState.data?.map((p) => (
            <InfoCard
              key={p.id}
              label={p.name}
              description={p.description}
              onClick={() => handlePersonaSelected(p)}
              className={`hover:bg-accent hover:cursor-pointer h-full w-full ${
                p.id === persona?.id ? "border-accent-foreground" : ""
              }`}
            />
          ))}
        </div>
      </div>

      <div className="mt-6">
        <p className="text-sm font-medium mb-2">Model</p>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {availableModels.map((m) => (
            <InfoCard
              key={m.id}
              label={m.name}
              description={m.modelIdentifier}
              onClick={() => handleModelSelected(m)}
              className={`hover:bg-accent hover:cursor-pointer h-full w-full ${
                m.id === model?.id ? "border-accent-foreground" : ""
              }`}
            />
          ))}
        </div>
      </div>

      <Button
        onClick={onBegin}
        disabled={beginDisabled}
        size="lg"
        className="w-full mt-6"
      >
        Begin Conversation
      </Button>
    </div>
  );
}
