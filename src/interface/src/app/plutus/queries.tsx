import { graphql } from "@/gql";

export const GET_ALL_MARKETS = graphql(`
    query GetMarkets {
        allMarkets {
            nodes {
                id
                name
                description
                icon
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

export const GET_RECENT_MARKET_TRADES = graphql(`
    query GetRecentMarketTrades($marketId: String!, $first: Int!) {
        allTrades(
            first: $first
            where: { metadata: { marketId: { eq: $marketId } } }
            order: { createdAt: DESC }
        ) {
            nodes {
                createdAt
                metadata {
                    symbolId
                    symbolName
                    symbolSubcode
                }
                price
                volume
            }
        }
    }
`)

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
        latestTrade: allTrades(
            first: 1
            order: { createdAt: DESC }
            where: { metadata: { symbolId: { eq: $symbolId } } }
        ) {
            nodes {
                price
                volume
            }
        }
        allForecasts(where: { symbolId: { eq: $symbolId } }) {
            nodes {
                predictions {
                    averagePrice
                }
                latest {
                    averagePrice
                }
            }
        }
        dailySymbolSummary(input: { symbolId: $symbolId }) {
            averagePrice
            maxPrice
            minPrice
            volume
        }
    }
`);

export const GET_RECIPE_TRADES = graphql(`
    query GetRecipeTrades($input: GetRecipeTradesInput!, $where: GetRecipeTradesResponseFilterInput, $order: [GetRecipeTradesResponseSortInput!], $first: Int, $after: String, $last: Int, $before: String) {
        recipeTrades(input: $input, where: $where, order: $order, first: $first, after: $after, last: $last, before: $before) {
            totalCount
            nodes {
                averageBuyPrice
                averageMargin
                averageSellPrice
                latestBuyPrice
                latestMargin
                latestSellPrice
                recipeId
                recipeName
            }
            pageInfo {
                endCursor
                hasNextPage
                hasPreviousPage
                startCursor
            }
        }
    }
`)

export const GET_MARKET_FORECAST = graphql(`
    query GetMarketForecast($marketId: String!, $where: GetMarketForecastResponseFilterInput, $order: [GetMarketForecastResponseSortInput!], $first: Int, $after: String, $last: Int, $before: String) {
        marketForecast(input: { marketId: $marketId }, where: $where, order: $order, first: $first, after: $after, last: $last, before: $before) {
            totalCount
            nodes {
                id
                symbolId
                symbolName
                symbolSubcode
                latest {
                    averagePrice
                }
                dayOne {
                    averagePrice
                    margin
                    gain
                }
                dayTwo {
                    averagePrice
                    margin
                    gain
                }
            }
            pageInfo {
                endCursor
                hasNextPage
                hasPreviousPage
                startCursor
            }
        }
    }
`)
