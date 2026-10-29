import {
  type ModelFormInput,
  type PersonaFormInput,
  type TraitFormInput,
} from "@/app/(hermes)/hermes/types";
import {
  type SavedConversationModel,
  type SavedConversationPersona,
  type SavedConversationTrait,
} from "@/lib/api/hermes";

/** Maps a saved conversation's persona onto the chat form's persona input. */
export function toPersonaFormInput(
  persona: SavedConversationPersona,
): PersonaFormInput {
  return {
    id: persona.id,
    name: persona.name,
    description: persona.description,
    personality: persona.personality,
    scenario: persona.scenario,
    isDefault: false,
    isPublic: false,
  };
}

/** Maps a saved conversation's model onto the chat form's model input. */
export function toModelFormInput(model: SavedConversationModel): ModelFormInput {
  return {
    id: model.id,
    name: model.name,
    modelIdentifier: model.modelIdentifier,
    systemPrompt: model.systemPrompt,
    temperature: model.temperature,
    maxTokens: model.maxTokens,
    defaultToolSetId: model.defaultToolSetId,
    contextWindow: model.contextWindow,
    isDefault: false,
    isPublic: false,
  };
}

/** Maps a saved conversation's traits onto the chat form's trait inputs. */
export function toTraitFormInputs(
  traits: SavedConversationTrait[],
): TraitFormInput[] {
  return traits.map((t) => ({
    id: t.id,
    name: t.name,
    content: t.content,
    isPublic: false,
    isEphemeral: false,
  }));
}
