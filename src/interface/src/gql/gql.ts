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
    "\n    mutation deleteCharacter($input: DeleteCharacterInput!) {\n        deleteCharacter(input: $input) {\n            idResponseOfCharacter {\n                id\n            }\n        }\n    } \n": types.DeleteCharacterDocument,
    "\n    mutation createCharacter($input: CreateCharacterInput!) {\n        createCharacter(input: $input) {\n            idResponseOfCharacter {\n                id\n            }\n        }\n    } \n": types.CreateCharacterDocument,
    "\n    mutation updateCharacter($input: UpdateCharacterInput!) {\n        updateCharacter(input: $input) {\n            idResponseOfCharacter {\n                id\n            }\n        }\n    } \n": types.UpdateCharacterDocument,
    "\n    mutation generateCompletion($input: GenerateCompletionInput!) {\n        generateCompletion(input: $input) {\n            completionResponse {\n                chunks @stream {\n                    content\n                }\n            }\n        }\n    }\n": types.GenerateCompletionDocument,
    "\n    query characterList {\n        allCharacters {\n            id\n            name\n            age\n        }\n    }\n": types.CharacterListDocument,
    "\n    query detailedCharacterList {\n        allCharacters {\n            id\n            name\n            age\n            details {\n                key\n                value\n            }\n        }\n    }\n": types.DetailedCharacterListDocument,
    "\n    query getCharacter($characterId: String!) {\n        character(characterId: $characterId) {\n            id\n            name\n            age\n            details {\n                key\n                value\n            }\n        }\n    }\n": types.GetCharacterDocument,
    "\n    query GetMarkets {\n        allMarkets {\n            nodes {\n                id\n                name\n            }\n        }\n    }\n": types.GetMarketsDocument,
    "\n    query GetMarketTrades($input: GetMarketTradesInput!, $where: GetMarketTradesResponseFilterInput, $order: [GetMarketTradesResponseSortInput!], $first: Int, $after: String, $last: Int, $before: String) {\n        marketTrades(input: $input, where: $where, order: $order, first: $first, after: $after, last: $last, before: $before) {\n            totalCount\n            nodes {\n                averagePrice\n                limit\n                margin\n                maxPrice\n                minPrice\n                numTransactions\n                roi\n                symbolCode\n                symbolId\n                symbolName\n                symbolSubcode\n                totalGain\n                totalSpent\n                totalVolume\n            }\n            pageInfo {\n                endCursor\n                hasNextPage\n                hasPreviousPage\n                startCursor\n            }\n        }\n    }\n": types.GetMarketTradesDocument,
    "\n    query GetRecentMarketTrades($marketId: String!, $first: Int!) {\n        allTrades(\n            first: $first\n            where: { metadata: { marketId: { eq: $marketId } } }\n            order: { createdAt: DESC }\n        ) {\n            nodes {\n                createdAt\n                metadata {\n                    symbolId\n                    symbolName\n                    symbolSubcode\n                }\n                price\n                volume\n            }\n        }\n    }\n": types.GetRecentMarketTradesDocument,
    "\n    query GetSymbolDetails($symbolId: String!, $seconds: Float) {\n        symbol(symbolId: $symbolId) {\n            code\n            createdAt\n            id\n            marketId\n            name\n            subcode\n            updatedAt\n        }\n        symbolTrades(input: { symbolId: $symbolId, seconds: $seconds }) {\n            totalSpent\n            averagePrice\n            minPrice\n            maxPrice\n            volume\n            numTransactions\n            trades {\n                date\n                maxPrice\n                minPrice\n                numTransactions\n                price\n                totalSpent\n                volume\n            }\n        }\n        latestTrade: allTrades(\n            first: 1\n            order: { createdAt: DESC }\n            where: { metadata: { symbolId: { eq: $symbolId } } }\n        ) {\n            nodes {\n                price\n                volume\n            }\n        }\n        allForecasts(where: { symbolId: { eq: $symbolId } }) {\n            nodes {\n                predictions {\n                    averagePrice\n                }\n                latest {\n                    averagePrice\n                }\n            }\n        }\n        dailySymbolSummary(input: { symbolId: $symbolId }) {\n            averagePrice\n            maxPrice\n            minPrice\n            volume\n        }\n    }\n": types.GetSymbolDetailsDocument,
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
export function graphql(source: "\n    mutation deleteCharacter($input: DeleteCharacterInput!) {\n        deleteCharacter(input: $input) {\n            idResponseOfCharacter {\n                id\n            }\n        }\n    } \n"): (typeof documents)["\n    mutation deleteCharacter($input: DeleteCharacterInput!) {\n        deleteCharacter(input: $input) {\n            idResponseOfCharacter {\n                id\n            }\n        }\n    } \n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    mutation createCharacter($input: CreateCharacterInput!) {\n        createCharacter(input: $input) {\n            idResponseOfCharacter {\n                id\n            }\n        }\n    } \n"): (typeof documents)["\n    mutation createCharacter($input: CreateCharacterInput!) {\n        createCharacter(input: $input) {\n            idResponseOfCharacter {\n                id\n            }\n        }\n    } \n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    mutation updateCharacter($input: UpdateCharacterInput!) {\n        updateCharacter(input: $input) {\n            idResponseOfCharacter {\n                id\n            }\n        }\n    } \n"): (typeof documents)["\n    mutation updateCharacter($input: UpdateCharacterInput!) {\n        updateCharacter(input: $input) {\n            idResponseOfCharacter {\n                id\n            }\n        }\n    } \n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    mutation generateCompletion($input: GenerateCompletionInput!) {\n        generateCompletion(input: $input) {\n            completionResponse {\n                chunks @stream {\n                    content\n                }\n            }\n        }\n    }\n"): (typeof documents)["\n    mutation generateCompletion($input: GenerateCompletionInput!) {\n        generateCompletion(input: $input) {\n            completionResponse {\n                chunks @stream {\n                    content\n                }\n            }\n        }\n    }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    query characterList {\n        allCharacters {\n            id\n            name\n            age\n        }\n    }\n"): (typeof documents)["\n    query characterList {\n        allCharacters {\n            id\n            name\n            age\n        }\n    }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    query detailedCharacterList {\n        allCharacters {\n            id\n            name\n            age\n            details {\n                key\n                value\n            }\n        }\n    }\n"): (typeof documents)["\n    query detailedCharacterList {\n        allCharacters {\n            id\n            name\n            age\n            details {\n                key\n                value\n            }\n        }\n    }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    query getCharacter($characterId: String!) {\n        character(characterId: $characterId) {\n            id\n            name\n            age\n            details {\n                key\n                value\n            }\n        }\n    }\n"): (typeof documents)["\n    query getCharacter($characterId: String!) {\n        character(characterId: $characterId) {\n            id\n            name\n            age\n            details {\n                key\n                value\n            }\n        }\n    }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    query GetMarkets {\n        allMarkets {\n            nodes {\n                id\n                name\n            }\n        }\n    }\n"): (typeof documents)["\n    query GetMarkets {\n        allMarkets {\n            nodes {\n                id\n                name\n            }\n        }\n    }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    query GetMarketTrades($input: GetMarketTradesInput!, $where: GetMarketTradesResponseFilterInput, $order: [GetMarketTradesResponseSortInput!], $first: Int, $after: String, $last: Int, $before: String) {\n        marketTrades(input: $input, where: $where, order: $order, first: $first, after: $after, last: $last, before: $before) {\n            totalCount\n            nodes {\n                averagePrice\n                limit\n                margin\n                maxPrice\n                minPrice\n                numTransactions\n                roi\n                symbolCode\n                symbolId\n                symbolName\n                symbolSubcode\n                totalGain\n                totalSpent\n                totalVolume\n            }\n            pageInfo {\n                endCursor\n                hasNextPage\n                hasPreviousPage\n                startCursor\n            }\n        }\n    }\n"): (typeof documents)["\n    query GetMarketTrades($input: GetMarketTradesInput!, $where: GetMarketTradesResponseFilterInput, $order: [GetMarketTradesResponseSortInput!], $first: Int, $after: String, $last: Int, $before: String) {\n        marketTrades(input: $input, where: $where, order: $order, first: $first, after: $after, last: $last, before: $before) {\n            totalCount\n            nodes {\n                averagePrice\n                limit\n                margin\n                maxPrice\n                minPrice\n                numTransactions\n                roi\n                symbolCode\n                symbolId\n                symbolName\n                symbolSubcode\n                totalGain\n                totalSpent\n                totalVolume\n            }\n            pageInfo {\n                endCursor\n                hasNextPage\n                hasPreviousPage\n                startCursor\n            }\n        }\n    }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    query GetRecentMarketTrades($marketId: String!, $first: Int!) {\n        allTrades(\n            first: $first\n            where: { metadata: { marketId: { eq: $marketId } } }\n            order: { createdAt: DESC }\n        ) {\n            nodes {\n                createdAt\n                metadata {\n                    symbolId\n                    symbolName\n                    symbolSubcode\n                }\n                price\n                volume\n            }\n        }\n    }\n"): (typeof documents)["\n    query GetRecentMarketTrades($marketId: String!, $first: Int!) {\n        allTrades(\n            first: $first\n            where: { metadata: { marketId: { eq: $marketId } } }\n            order: { createdAt: DESC }\n        ) {\n            nodes {\n                createdAt\n                metadata {\n                    symbolId\n                    symbolName\n                    symbolSubcode\n                }\n                price\n                volume\n            }\n        }\n    }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    query GetSymbolDetails($symbolId: String!, $seconds: Float) {\n        symbol(symbolId: $symbolId) {\n            code\n            createdAt\n            id\n            marketId\n            name\n            subcode\n            updatedAt\n        }\n        symbolTrades(input: { symbolId: $symbolId, seconds: $seconds }) {\n            totalSpent\n            averagePrice\n            minPrice\n            maxPrice\n            volume\n            numTransactions\n            trades {\n                date\n                maxPrice\n                minPrice\n                numTransactions\n                price\n                totalSpent\n                volume\n            }\n        }\n        latestTrade: allTrades(\n            first: 1\n            order: { createdAt: DESC }\n            where: { metadata: { symbolId: { eq: $symbolId } } }\n        ) {\n            nodes {\n                price\n                volume\n            }\n        }\n        allForecasts(where: { symbolId: { eq: $symbolId } }) {\n            nodes {\n                predictions {\n                    averagePrice\n                }\n                latest {\n                    averagePrice\n                }\n            }\n        }\n        dailySymbolSummary(input: { symbolId: $symbolId }) {\n            averagePrice\n            maxPrice\n            minPrice\n            volume\n        }\n    }\n"): (typeof documents)["\n    query GetSymbolDetails($symbolId: String!, $seconds: Float) {\n        symbol(symbolId: $symbolId) {\n            code\n            createdAt\n            id\n            marketId\n            name\n            subcode\n            updatedAt\n        }\n        symbolTrades(input: { symbolId: $symbolId, seconds: $seconds }) {\n            totalSpent\n            averagePrice\n            minPrice\n            maxPrice\n            volume\n            numTransactions\n            trades {\n                date\n                maxPrice\n                minPrice\n                numTransactions\n                price\n                totalSpent\n                volume\n            }\n        }\n        latestTrade: allTrades(\n            first: 1\n            order: { createdAt: DESC }\n            where: { metadata: { symbolId: { eq: $symbolId } } }\n        ) {\n            nodes {\n                price\n                volume\n            }\n        }\n        allForecasts(where: { symbolId: { eq: $symbolId } }) {\n            nodes {\n                predictions {\n                    averagePrice\n                }\n                latest {\n                    averagePrice\n                }\n            }\n        }\n        dailySymbolSummary(input: { symbolId: $symbolId }) {\n            averagePrice\n            maxPrice\n            minPrice\n            volume\n        }\n    }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;