import { graphql } from "@/gql";

export const getAllMarketsQuery = graphql(`
    query GetMarkets {
        allMarkets {
            nodes {
                id
                name
            }
        }
    }
`);

export const getMarketTradesQuery = graphql(`
    query GetMarketTrades($input: GetMarketTradesInput!, $where: GetMarketTradesResponseFilterInput, $order: [GetMarketTradesResponseSortInput!], $first: Int, $after: String, $last: Int, $before: String) {
        marketTrades(input: $input, where: $where, order: $order, first: $first, after: $after, last: $last, before: $before) {
            totalCount
            nodes {
                averagePrice
                limit
                margin
                maxPrice
                minPrice
                numTransactions
                roi
                symbolCode
                symbolId
                symbolName
                symbolSubcode
                totalGain
                totalSpent
                totalVolume
            }
            pageInfo {
                endCursor
                hasNextPage
                hasPreviousPage
                startCursor
            }
        }
    }
`);

export const getSymbolDetailsQuery = graphql(`
    query GetSymbolDetails($marketId: String!, $symbolId: String!, $seconds: Float) {
        symbol(symbolId: $symbolId) {
            code
            createdAt
            id
            marketId
            name
            subcode
            updatedAt
        }
        symbolTrades(input: { marketId: $marketId, symbolId: $symbolId, seconds: $seconds }) {
            averageGain
            averagePrice
            margin
            maxPrice
            minPrice
            numTransactions
            tax
            totalGain
            totalSpent
            trades {
                date
                margin
                maxPrice
                minPrice
                numTransactions
                price
                totalSpent
                volume
            }
        }
    }
`);
