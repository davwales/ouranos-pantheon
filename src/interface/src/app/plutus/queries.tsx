import { graphql } from "@/gql";

export const GET_ALL_MARKETS = graphql(`
    query GetMarkets {
        allMarkets {
            nodes {
                id
                name
            }
        }
    }
`);

export const GET_MARKET_TRADES = graphql(`
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

export const GET_SYMBOL_DETAILS = graphql(`
    query GetSymbolDetails($symbolId: String!, $seconds: Float) {
        symbol(symbolId: $symbolId) {
            code
            createdAt
            id
            marketId
            name
            subcode
            updatedAt
        }
        symbolTrades(input: { symbolId: $symbolId, seconds: $seconds }) {
            totalSpent
            averagePrice
            minPrice
            maxPrice
            volume
            numTransactions
            trades {
                date
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
