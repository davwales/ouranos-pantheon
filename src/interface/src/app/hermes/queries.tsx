import { graphql } from "@/gql";

export const GET_CHARACTER_LIST = graphql(`
    query characterList {
        allCharacters {
            id
            name
            age
        }
    }
`);

export const GET_DETAILED_CHARACTER_LIST = graphql(`
    query detailedCharacterList {
        allCharacters {
            id
            name
            age
            details {
                key
                value
            }
        }
    }
`);

export const GET_CHARACTER = graphql(`
    query getCharacter($characterId: String!) {
        character(characterId: $characterId) {
            id
            name
            age
            details {
                key
                value
            }
        }
    }
`);
