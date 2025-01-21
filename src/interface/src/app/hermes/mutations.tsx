import { graphql } from "@/gql";

export const deleteCharacterMutation = graphql(`
    mutation deleteCharacter($input: DeleteCharacterInput!) {
        deleteCharacter(input: $input) {
            idResponseOfCharacter {
                id
            }
        }
    } 
`);

export const createCharacterMutation = graphql(`
    mutation createCharacter($input: CreateCharacterInput!) {
        createCharacter(input: $input) {
            idResponseOfCharacter {
                id
            }
        }
    } 
`);

export const updateCharacterMutation = graphql(`
    mutation updateCharacter($input: UpdateCharacterInput!) {
        updateCharacter(input: $input) {
            idResponseOfCharacter {
                id
            }
        }
    } 
`);

export const generateCompletion = graphql(`
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
