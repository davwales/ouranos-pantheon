import { graphql } from "@/gql";

export const DELETE_CHARACTER = graphql(`
    mutation deleteCharacter($input: DeleteCharacterInput!) {
        deleteCharacter(input: $input) {
            idResponseOfCharacter {
                id
            }
        }
    } 
`);

export const CREATE_CHARACTER = graphql(`
    mutation createCharacter($input: CreateCharacterInput!) {
        createCharacter(input: $input) {
            idResponseOfCharacter {
                id
            }
        }
    } 
`);

export const UPDATE_CHARACTER = graphql(`
    mutation updateCharacter($input: UpdateCharacterInput!) {
        updateCharacter(input: $input) {
            idResponseOfCharacter {
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
