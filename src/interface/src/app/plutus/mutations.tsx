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

export const DELETE_RECIPE = graphql(`
  mutation DeleteRecipe($input: DeleteRecipeInput!) {
    deleteRecipe(input: $input) {
      idResponseOfRecipe {
        id
      }
    }
  }
`);
