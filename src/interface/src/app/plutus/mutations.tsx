import { graphql } from "@/gql";

export const UPDATE_RECIPE = graphql(`
    mutation UpdateRecipe($input: UpdateRecipeInput!) {
        updateRecipe(input: $input) {
            idResponseOfRecipe {
                id
            }
        }
    } 
`);