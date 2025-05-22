import { graphql } from "@/gql";

export const DELETE_ASSISTANT = graphql(`
  mutation deleteAssistant($input: DeleteAssistantInput!) {
    deleteAssistant(input: $input) {
      idResponseOfAssistant {
        id
      }
    }
  }
`);

export const CREATE_ASSISTANT = graphql(`
  mutation createAssistant($input: CreateAssistantInput!) {
    createAssistant(input: $input) {
      idResponseOfAssistant {
        id
      }
    }
  }
`);

export const UPDATE_ASSISTANT = graphql(`
  mutation updateAssistant($input: UpdateAssistantInput!) {
    updateAssistant(input: $input) {
      idResponseOfAssistant {
        id
      }
    }
  }
`);

export const GENERATE_COMPLETION = graphql(`
  mutation generateCompletion($input: GenerateCompletionInput!) {
    generateCompletion(input: $input) {
      completionResponse {
        chunks @stream {
          content
        }
      }
    }
  }
`);
