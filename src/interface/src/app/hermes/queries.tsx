import { graphql } from "@/gql";

export const GET_ASSISTANT_LIST = graphql(`
  query AssistantList {
    allAssistants {
      id
      model
      assistantName
    }
  }
`);

export const GET_DETAILED_ASSISTANT_LIST = graphql(`
  query DetailedAssistantList {
    allAssistants {
      assistantName
      createdAt
      id
      maxTokens
      model
      repeatPenalty
      systemPrompt
      temperature
      updatedAt
      userName
    }
  }
`);

export const GET_ASSISTANT = graphql(`
  query GetAssistant($assistantId: String!) {
    assistant(assistantId: $assistantId) {
      assistantName
      id
      maxTokens
      model
      repeatPenalty
      systemPrompt
      temperature
      userName
    }
  }
`);
