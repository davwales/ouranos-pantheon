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

export const getDetailedCharacterListQuery = graphql(`
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
