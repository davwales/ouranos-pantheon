/* eslint-disable */
import * as types from './graphql';
import { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';

/**
 * Map of all GraphQL operations in the project.
 *
 * This map has several performance disadvantages:
 * 1. It is not tree-shakeable, so it will include all operations in the project.
 * 2. It is not minifiable, so the string of a GraphQL query will be multiple times inside the bundle.
 * 3. It does not support dead code elimination, so it will add unused operations.
 *
 * Therefore it is highly recommended to use the babel or swc plugin for production.
 */
const documents = {
    "\n  mutation deleteAssistant($input: DeleteAssistantInput!) {\n    deleteAssistant(input: $input) {\n      idResponseOfAssistant {\n        id\n      }\n    }\n  }\n": types.DeleteAssistantDocument,
    "\n  mutation createAssistant($input: CreateAssistantInput!) {\n    createAssistant(input: $input) {\n      idResponseOfAssistant {\n        id\n      }\n    }\n  }\n": types.CreateAssistantDocument,
    "\n  mutation updateAssistant($input: UpdateAssistantInput!) {\n    updateAssistant(input: $input) {\n      idResponseOfAssistant {\n        id\n      }\n    }\n  }\n": types.UpdateAssistantDocument,
    "\n  mutation generateCompletion($input: GenerateCompletionInput!) {\n    generateCompletion(input: $input) {\n      completionResponse {\n        chunks @stream {\n          content\n        }\n      }\n    }\n  }\n": types.GenerateCompletionDocument,
    "\n  query AssistantList {\n    allAssistants {\n      id\n      model\n      assistantName\n    }\n  }\n": types.AssistantListDocument,
    "\n  query DetailedAssistantList {\n    allAssistants {\n      assistantName\n      createdAt\n      id\n      maxTokens\n      model\n      repeatPenalty\n      systemPrompt\n      temperature\n      updatedAt\n      userName\n    }\n  }\n": types.DetailedAssistantListDocument,
    "\n  query GetAssistant($assistantId: String!) {\n    assistant(assistantId: $assistantId) {\n      assistantName\n      id\n      maxTokens\n      model\n      repeatPenalty\n      systemPrompt\n      temperature\n      userName\n    }\n  }\n": types.GetAssistantDocument,
    "\n  mutation UpdateRecipe($input: UpdateRecipeInput!) {\n    updateRecipe(input: $input) {\n      idResponseOfRecipe {\n        id\n      }\n    }\n  }\n": types.UpdateRecipeDocument,
    "\n  mutation DeleteRecipe($input: DeleteRecipeInput!) {\n    deleteRecipe(input: $input) {\n      idResponseOfRecipe {\n        id\n      }\n    }\n  }\n": types.DeleteRecipeDocument,
    "\n  mutation CreateRecipe($input: CreateRecipeInput!) {\n    createRecipe(input: $input) {\n      idResponseOfRecipe {\n        id\n      }\n    }\n  }\n": types.CreateRecipeDocument,
    "\n  query GetMarkets {\n    allMarkets {\n      nodes {\n        id\n        name\n        description\n        icon\n      }\n    }\n  }\n": types.GetMarketsDocument,
    "\n  query GetMarketTrades(\n    $input: GetMarketTradesInput!\n    $where: GetMarketTradesResponseFilterInput\n    $order: [GetMarketTradesResponseSortInput!]\n    $first: Int\n    $after: String\n    $last: Int\n    $before: String\n  ) {\n    marketTrades(\n      input: $input\n      where: $where\n      order: $order\n      first: $first\n      after: $after\n      last: $last\n      before: $before\n    ) {\n      totalCount\n      nodes {\n        averagePrice\n        limit\n        margin\n        maxPrice\n        minPrice\n        numTransactions\n        roi\n        symbolCode\n        symbolId\n        symbolName\n        symbolSubcode\n        totalGain\n        totalSpent\n        totalVolume\n      }\n      pageInfo {\n        endCursor\n        hasNextPage\n        hasPreviousPage\n        startCursor\n      }\n    }\n  }\n": types.GetMarketTradesDocument,
    "\n  query GetRecentMarketTrades($marketId: String!, $first: Int!) {\n    allTrades(\n      first: $first\n      where: { metadata: { marketId: { eq: $marketId } } }\n      order: { createdAt: DESC }\n    ) {\n      nodes {\n        createdAt\n        metadata {\n          symbolId\n          symbolName\n          symbolSubcode\n        }\n        price\n        volume\n      }\n    }\n  }\n": types.GetRecentMarketTradesDocument,
    "\n  query GetSymbolDetails($symbolId: String!, $seconds: Float) {\n    symbol(symbolId: $symbolId) {\n      code\n      createdAt\n      id\n      marketId\n      name\n      subcode\n      updatedAt\n    }\n    symbolTrades(input: { symbolId: $symbolId, seconds: $seconds }) {\n      totalSpent\n      averagePrice\n      minPrice\n      maxPrice\n      volume\n      numTransactions\n      trades {\n        date\n        maxPrice\n        minPrice\n        numTransactions\n        price\n        totalSpent\n        volume\n      }\n    }\n    latestTrade: allTrades(\n      first: 1\n      order: { createdAt: DESC }\n      where: { metadata: { symbolId: { eq: $symbolId } } }\n    ) {\n      nodes {\n        price\n        volume\n      }\n    }\n    allForecasts(where: { symbolId: { eq: $symbolId } }) {\n      nodes {\n        predictions {\n          averagePrice\n        }\n        latest {\n          averagePrice\n        }\n      }\n    }\n    dailySymbolSummary(input: { symbolId: $symbolId }) {\n      averagePrice\n      maxPrice\n      minPrice\n      volume\n    }\n  }\n": types.GetSymbolDetailsDocument,
    "\n  query GetRecipeTrades(\n    $input: GetRecipeTradesInput!\n    $where: GetRecipeTradesResponseFilterInput\n    $order: [GetRecipeTradesResponseSortInput!]\n    $first: Int\n    $after: String\n    $last: Int\n    $before: String\n  ) {\n    recipeTrades(\n      input: $input\n      where: $where\n      order: $order\n      first: $first\n      after: $after\n      last: $last\n      before: $before\n    ) {\n      totalCount\n      nodes {\n        averageBuyPrice\n        averageMargin\n        averageSellPrice\n        latestBuyPrice\n        latestMargin\n        latestSellPrice\n        recipeId\n        recipeName\n      }\n      pageInfo {\n        endCursor\n        hasNextPage\n        hasPreviousPage\n        startCursor\n      }\n    }\n  }\n": types.GetRecipeTradesDocument,
    "\n  query GetMarketForecast(\n    $marketId: String!\n    $where: GetMarketForecastResponseFilterInput\n    $order: [GetMarketForecastResponseSortInput!]\n    $first: Int\n    $after: String\n    $last: Int\n    $before: String\n  ) {\n    marketForecast(\n      input: { marketId: $marketId }\n      where: $where\n      order: $order\n      first: $first\n      after: $after\n      last: $last\n      before: $before\n    ) {\n      totalCount\n      nodes {\n        id\n        symbolId\n        symbolName\n        symbolSubcode\n        latest {\n          averagePrice\n        }\n        dayOne {\n          averagePrice\n          margin\n          gain\n        }\n        dayTwo {\n          averagePrice\n          margin\n          gain\n        }\n      }\n      pageInfo {\n        endCursor\n        hasNextPage\n        hasPreviousPage\n        startCursor\n      }\n    }\n  }\n": types.GetMarketForecastDocument,
    "\n  query GetRecipeDetails($recipeId: String!) {\n    recipe(recipeId: $recipeId) {\n      id\n      name\n      cost\n      inputs {\n        name\n        quantity\n        symbolId\n      }\n      outputs {\n        name\n        quantity\n        symbolId\n      }\n    }\n  }\n": types.GetRecipeDetailsDocument,
    "\n  query SearchSymbols($marketId: String!, $query: String!) {\n    allSymbols(\n      where: { name: { contains: $query }, marketId: { eq: $marketId } }\n    ) {\n      nodes {\n        id\n        name\n        code\n        subcode\n      }\n    }\n  }\n": types.SearchSymbolsDocument,
};

/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 *
 *
 * @example
 * ```ts
 * const query = graphql(`query GetUser($id: ID!) { user(id: $id) { name } }`);
 * ```
 *
 * The query argument is unknown!
 * Please regenerate the types.
 */
export function graphql(source: string): unknown;

/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation deleteAssistant($input: DeleteAssistantInput!) {\n    deleteAssistant(input: $input) {\n      idResponseOfAssistant {\n        id\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation deleteAssistant($input: DeleteAssistantInput!) {\n    deleteAssistant(input: $input) {\n      idResponseOfAssistant {\n        id\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation createAssistant($input: CreateAssistantInput!) {\n    createAssistant(input: $input) {\n      idResponseOfAssistant {\n        id\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation createAssistant($input: CreateAssistantInput!) {\n    createAssistant(input: $input) {\n      idResponseOfAssistant {\n        id\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation updateAssistant($input: UpdateAssistantInput!) {\n    updateAssistant(input: $input) {\n      idResponseOfAssistant {\n        id\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation updateAssistant($input: UpdateAssistantInput!) {\n    updateAssistant(input: $input) {\n      idResponseOfAssistant {\n        id\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation generateCompletion($input: GenerateCompletionInput!) {\n    generateCompletion(input: $input) {\n      completionResponse {\n        chunks @stream {\n          content\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation generateCompletion($input: GenerateCompletionInput!) {\n    generateCompletion(input: $input) {\n      completionResponse {\n        chunks @stream {\n          content\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query AssistantList {\n    allAssistants {\n      id\n      model\n      assistantName\n    }\n  }\n"): (typeof documents)["\n  query AssistantList {\n    allAssistants {\n      id\n      model\n      assistantName\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query DetailedAssistantList {\n    allAssistants {\n      assistantName\n      createdAt\n      id\n      maxTokens\n      model\n      repeatPenalty\n      systemPrompt\n      temperature\n      updatedAt\n      userName\n    }\n  }\n"): (typeof documents)["\n  query DetailedAssistantList {\n    allAssistants {\n      assistantName\n      createdAt\n      id\n      maxTokens\n      model\n      repeatPenalty\n      systemPrompt\n      temperature\n      updatedAt\n      userName\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAssistant($assistantId: String!) {\n    assistant(assistantId: $assistantId) {\n      assistantName\n      id\n      maxTokens\n      model\n      repeatPenalty\n      systemPrompt\n      temperature\n      userName\n    }\n  }\n"): (typeof documents)["\n  query GetAssistant($assistantId: String!) {\n    assistant(assistantId: $assistantId) {\n      assistantName\n      id\n      maxTokens\n      model\n      repeatPenalty\n      systemPrompt\n      temperature\n      userName\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateRecipe($input: UpdateRecipeInput!) {\n    updateRecipe(input: $input) {\n      idResponseOfRecipe {\n        id\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation UpdateRecipe($input: UpdateRecipeInput!) {\n    updateRecipe(input: $input) {\n      idResponseOfRecipe {\n        id\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteRecipe($input: DeleteRecipeInput!) {\n    deleteRecipe(input: $input) {\n      idResponseOfRecipe {\n        id\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation DeleteRecipe($input: DeleteRecipeInput!) {\n    deleteRecipe(input: $input) {\n      idResponseOfRecipe {\n        id\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateRecipe($input: CreateRecipeInput!) {\n    createRecipe(input: $input) {\n      idResponseOfRecipe {\n        id\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation CreateRecipe($input: CreateRecipeInput!) {\n    createRecipe(input: $input) {\n      idResponseOfRecipe {\n        id\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetMarkets {\n    allMarkets {\n      nodes {\n        id\n        name\n        description\n        icon\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetMarkets {\n    allMarkets {\n      nodes {\n        id\n        name\n        description\n        icon\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetMarketTrades(\n    $input: GetMarketTradesInput!\n    $where: GetMarketTradesResponseFilterInput\n    $order: [GetMarketTradesResponseSortInput!]\n    $first: Int\n    $after: String\n    $last: Int\n    $before: String\n  ) {\n    marketTrades(\n      input: $input\n      where: $where\n      order: $order\n      first: $first\n      after: $after\n      last: $last\n      before: $before\n    ) {\n      totalCount\n      nodes {\n        averagePrice\n        limit\n        margin\n        maxPrice\n        minPrice\n        numTransactions\n        roi\n        symbolCode\n        symbolId\n        symbolName\n        symbolSubcode\n        totalGain\n        totalSpent\n        totalVolume\n      }\n      pageInfo {\n        endCursor\n        hasNextPage\n        hasPreviousPage\n        startCursor\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetMarketTrades(\n    $input: GetMarketTradesInput!\n    $where: GetMarketTradesResponseFilterInput\n    $order: [GetMarketTradesResponseSortInput!]\n    $first: Int\n    $after: String\n    $last: Int\n    $before: String\n  ) {\n    marketTrades(\n      input: $input\n      where: $where\n      order: $order\n      first: $first\n      after: $after\n      last: $last\n      before: $before\n    ) {\n      totalCount\n      nodes {\n        averagePrice\n        limit\n        margin\n        maxPrice\n        minPrice\n        numTransactions\n        roi\n        symbolCode\n        symbolId\n        symbolName\n        symbolSubcode\n        totalGain\n        totalSpent\n        totalVolume\n      }\n      pageInfo {\n        endCursor\n        hasNextPage\n        hasPreviousPage\n        startCursor\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetRecentMarketTrades($marketId: String!, $first: Int!) {\n    allTrades(\n      first: $first\n      where: { metadata: { marketId: { eq: $marketId } } }\n      order: { createdAt: DESC }\n    ) {\n      nodes {\n        createdAt\n        metadata {\n          symbolId\n          symbolName\n          symbolSubcode\n        }\n        price\n        volume\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetRecentMarketTrades($marketId: String!, $first: Int!) {\n    allTrades(\n      first: $first\n      where: { metadata: { marketId: { eq: $marketId } } }\n      order: { createdAt: DESC }\n    ) {\n      nodes {\n        createdAt\n        metadata {\n          symbolId\n          symbolName\n          symbolSubcode\n        }\n        price\n        volume\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetSymbolDetails($symbolId: String!, $seconds: Float) {\n    symbol(symbolId: $symbolId) {\n      code\n      createdAt\n      id\n      marketId\n      name\n      subcode\n      updatedAt\n    }\n    symbolTrades(input: { symbolId: $symbolId, seconds: $seconds }) {\n      totalSpent\n      averagePrice\n      minPrice\n      maxPrice\n      volume\n      numTransactions\n      trades {\n        date\n        maxPrice\n        minPrice\n        numTransactions\n        price\n        totalSpent\n        volume\n      }\n    }\n    latestTrade: allTrades(\n      first: 1\n      order: { createdAt: DESC }\n      where: { metadata: { symbolId: { eq: $symbolId } } }\n    ) {\n      nodes {\n        price\n        volume\n      }\n    }\n    allForecasts(where: { symbolId: { eq: $symbolId } }) {\n      nodes {\n        predictions {\n          averagePrice\n        }\n        latest {\n          averagePrice\n        }\n      }\n    }\n    dailySymbolSummary(input: { symbolId: $symbolId }) {\n      averagePrice\n      maxPrice\n      minPrice\n      volume\n    }\n  }\n"): (typeof documents)["\n  query GetSymbolDetails($symbolId: String!, $seconds: Float) {\n    symbol(symbolId: $symbolId) {\n      code\n      createdAt\n      id\n      marketId\n      name\n      subcode\n      updatedAt\n    }\n    symbolTrades(input: { symbolId: $symbolId, seconds: $seconds }) {\n      totalSpent\n      averagePrice\n      minPrice\n      maxPrice\n      volume\n      numTransactions\n      trades {\n        date\n        maxPrice\n        minPrice\n        numTransactions\n        price\n        totalSpent\n        volume\n      }\n    }\n    latestTrade: allTrades(\n      first: 1\n      order: { createdAt: DESC }\n      where: { metadata: { symbolId: { eq: $symbolId } } }\n    ) {\n      nodes {\n        price\n        volume\n      }\n    }\n    allForecasts(where: { symbolId: { eq: $symbolId } }) {\n      nodes {\n        predictions {\n          averagePrice\n        }\n        latest {\n          averagePrice\n        }\n      }\n    }\n    dailySymbolSummary(input: { symbolId: $symbolId }) {\n      averagePrice\n      maxPrice\n      minPrice\n      volume\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetRecipeTrades(\n    $input: GetRecipeTradesInput!\n    $where: GetRecipeTradesResponseFilterInput\n    $order: [GetRecipeTradesResponseSortInput!]\n    $first: Int\n    $after: String\n    $last: Int\n    $before: String\n  ) {\n    recipeTrades(\n      input: $input\n      where: $where\n      order: $order\n      first: $first\n      after: $after\n      last: $last\n      before: $before\n    ) {\n      totalCount\n      nodes {\n        averageBuyPrice\n        averageMargin\n        averageSellPrice\n        latestBuyPrice\n        latestMargin\n        latestSellPrice\n        recipeId\n        recipeName\n      }\n      pageInfo {\n        endCursor\n        hasNextPage\n        hasPreviousPage\n        startCursor\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetRecipeTrades(\n    $input: GetRecipeTradesInput!\n    $where: GetRecipeTradesResponseFilterInput\n    $order: [GetRecipeTradesResponseSortInput!]\n    $first: Int\n    $after: String\n    $last: Int\n    $before: String\n  ) {\n    recipeTrades(\n      input: $input\n      where: $where\n      order: $order\n      first: $first\n      after: $after\n      last: $last\n      before: $before\n    ) {\n      totalCount\n      nodes {\n        averageBuyPrice\n        averageMargin\n        averageSellPrice\n        latestBuyPrice\n        latestMargin\n        latestSellPrice\n        recipeId\n        recipeName\n      }\n      pageInfo {\n        endCursor\n        hasNextPage\n        hasPreviousPage\n        startCursor\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetMarketForecast(\n    $marketId: String!\n    $where: GetMarketForecastResponseFilterInput\n    $order: [GetMarketForecastResponseSortInput!]\n    $first: Int\n    $after: String\n    $last: Int\n    $before: String\n  ) {\n    marketForecast(\n      input: { marketId: $marketId }\n      where: $where\n      order: $order\n      first: $first\n      after: $after\n      last: $last\n      before: $before\n    ) {\n      totalCount\n      nodes {\n        id\n        symbolId\n        symbolName\n        symbolSubcode\n        latest {\n          averagePrice\n        }\n        dayOne {\n          averagePrice\n          margin\n          gain\n        }\n        dayTwo {\n          averagePrice\n          margin\n          gain\n        }\n      }\n      pageInfo {\n        endCursor\n        hasNextPage\n        hasPreviousPage\n        startCursor\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetMarketForecast(\n    $marketId: String!\n    $where: GetMarketForecastResponseFilterInput\n    $order: [GetMarketForecastResponseSortInput!]\n    $first: Int\n    $after: String\n    $last: Int\n    $before: String\n  ) {\n    marketForecast(\n      input: { marketId: $marketId }\n      where: $where\n      order: $order\n      first: $first\n      after: $after\n      last: $last\n      before: $before\n    ) {\n      totalCount\n      nodes {\n        id\n        symbolId\n        symbolName\n        symbolSubcode\n        latest {\n          averagePrice\n        }\n        dayOne {\n          averagePrice\n          margin\n          gain\n        }\n        dayTwo {\n          averagePrice\n          margin\n          gain\n        }\n      }\n      pageInfo {\n        endCursor\n        hasNextPage\n        hasPreviousPage\n        startCursor\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetRecipeDetails($recipeId: String!) {\n    recipe(recipeId: $recipeId) {\n      id\n      name\n      cost\n      inputs {\n        name\n        quantity\n        symbolId\n      }\n      outputs {\n        name\n        quantity\n        symbolId\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetRecipeDetails($recipeId: String!) {\n    recipe(recipeId: $recipeId) {\n      id\n      name\n      cost\n      inputs {\n        name\n        quantity\n        symbolId\n      }\n      outputs {\n        name\n        quantity\n        symbolId\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query SearchSymbols($marketId: String!, $query: String!) {\n    allSymbols(\n      where: { name: { contains: $query }, marketId: { eq: $marketId } }\n    ) {\n      nodes {\n        id\n        name\n        code\n        subcode\n      }\n    }\n  }\n"): (typeof documents)["\n  query SearchSymbols($marketId: String!, $query: String!) {\n    allSymbols(\n      where: { name: { contains: $query }, marketId: { eq: $marketId } }\n    ) {\n      nodes {\n        id\n        name\n        code\n        subcode\n      }\n    }\n  }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;