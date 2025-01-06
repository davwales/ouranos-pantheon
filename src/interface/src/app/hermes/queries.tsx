import { graphql } from "@/gql";

export const getCharacterListQuery = graphql(`
    query characterList {
        allCharacters {
            id
            name
            age
        }
    }
`);

export const getCharacterQuery = graphql(`
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
