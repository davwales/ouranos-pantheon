/* eslint-disable */
import { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';
export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
export type MakeEmpty<T extends { [key: string]: unknown }, K extends keyof T> = { [_ in K]?: never };
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
  /** The `DateTime` scalar represents an ISO-8601 compliant date time type. */
  DateTime: { input: any; output: any; }
  /** The `Decimal` scalar type represents a decimal floating-point number. */
  Decimal: { input: any; output: any; }
};

export type AdditionalFields = {
  __typename?: 'AdditionalFields';
  exchange?: Maybe<Scalars['String']['output']>;
  externalTradeId?: Maybe<Scalars['String']['output']>;
  highAlch?: Maybe<Scalars['Int']['output']>;
  limit?: Maybe<Scalars['Decimal']['output']>;
  lowAlch?: Maybe<Scalars['Int']['output']>;
  tape?: Maybe<Scalars['String']['output']>;
};

export type AdditionalFieldsFilterInput = {
  and?: InputMaybe<Array<AdditionalFieldsFilterInput>>;
  exchange?: InputMaybe<StringOperationFilterInput>;
  externalTradeId?: InputMaybe<StringOperationFilterInput>;
  highAlch?: InputMaybe<IntOperationFilterInput>;
  limit?: InputMaybe<DecimalOperationFilterInput>;
  lowAlch?: InputMaybe<IntOperationFilterInput>;
  or?: InputMaybe<Array<AdditionalFieldsFilterInput>>;
  tape?: InputMaybe<StringOperationFilterInput>;
};

export type AdditionalFieldsSortInput = {
  exchange?: InputMaybe<SortEnumType>;
  externalTradeId?: InputMaybe<SortEnumType>;
  highAlch?: InputMaybe<SortEnumType>;
  limit?: InputMaybe<SortEnumType>;
  lowAlch?: InputMaybe<SortEnumType>;
  tape?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type AllForecastsConnection = {
  __typename?: 'AllForecastsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<AllForecastsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Forecast>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type AllForecastsEdge = {
  __typename?: 'AllForecastsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Forecast;
};

/** A connection to a list of items. */
export type AllMarketsConnection = {
  __typename?: 'AllMarketsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<AllMarketsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Market>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type AllMarketsEdge = {
  __typename?: 'AllMarketsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Market;
};

/** A connection to a list of items. */
export type AllRecipesConnection = {
  __typename?: 'AllRecipesConnection';
  /** A list of edges. */
  edges?: Maybe<Array<AllRecipesEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Recipe>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type AllRecipesEdge = {
  __typename?: 'AllRecipesEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Recipe;
};

/** A connection to a list of items. */
export type AllSymbolGroupsConnection = {
  __typename?: 'AllSymbolGroupsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<AllSymbolGroupsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<SymbolGroup>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type AllSymbolGroupsEdge = {
  __typename?: 'AllSymbolGroupsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: SymbolGroup;
};

/** A connection to a list of items. */
export type AllSymbolsConnection = {
  __typename?: 'AllSymbolsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<AllSymbolsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Symbol>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type AllSymbolsEdge = {
  __typename?: 'AllSymbolsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Symbol;
};

/** A connection to a list of items. */
export type AllTradesConnection = {
  __typename?: 'AllTradesConnection';
  /** A list of edges. */
  edges?: Maybe<Array<AllTradesEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Trade>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
};

/** An edge in a connection. */
export type AllTradesEdge = {
  __typename?: 'AllTradesEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Trade;
};

export type BooleanOperationFilterInput = {
  eq?: InputMaybe<Scalars['Boolean']['input']>;
  neq?: InputMaybe<Scalars['Boolean']['input']>;
};

export type Character = {
  __typename?: 'Character';
  age: Scalars['Int']['output'];
  createdAt: Scalars['DateTime']['output'];
  details: Array<CharacterDetail>;
  id: Scalars['String']['output'];
  name: Scalars['String']['output'];
  updatedAt: Scalars['DateTime']['output'];
};

export type CharacterDetail = {
  __typename?: 'CharacterDetail';
  key: Scalars['String']['output'];
  value: Scalars['String']['output'];
};

export type CharacterDetailFilterInput = {
  and?: InputMaybe<Array<CharacterDetailFilterInput>>;
  key?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<CharacterDetailFilterInput>>;
  value?: InputMaybe<StringOperationFilterInput>;
};

export type CharacterDetailInput = {
  key: Scalars['String']['input'];
  value: Scalars['String']['input'];
};

export type CharacterFilterInput = {
  age?: InputMaybe<IntOperationFilterInput>;
  and?: InputMaybe<Array<CharacterFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  details?: InputMaybe<ListFilterInputTypeOfCharacterDetailFilterInput>;
  id?: InputMaybe<IdFilterInputTypeOfCharacterFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<CharacterFilterInput>>;
  updatedAt?: InputMaybe<DateTimeOperationFilterInput>;
};

export type CharacterInput = {
  age: Scalars['Int']['input'];
  details: Array<CharacterDetailInput>;
  name: Scalars['String']['input'];
};

export type CharacterSortInput = {
  age?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
  updatedAt?: InputMaybe<SortEnumType>;
};

export type CompletionResponse = {
  __typename?: 'CompletionResponse';
  chunks: Array<GenerateCompletionResponse>;
};

export type ConversationInput = {
  assistant: CharacterInput;
  context: Scalars['String']['input'];
  messages: Array<MessageInput>;
  user: CharacterInput;
};

export type CreateCharacterInput = {
  age: Scalars['Int']['input'];
  details: Array<CharacterDetailInput>;
  name: Scalars['String']['input'];
};

export type CreateCharacterPayload = {
  __typename?: 'CreateCharacterPayload';
  idResponseOfCharacter?: Maybe<IdResponseOfCharacter>;
};

export type CreateMarketInput = {
  name: Scalars['String']['input'];
  taxes: TaxesInput;
};

export type CreateMarketPayload = {
  __typename?: 'CreateMarketPayload';
  idResponseOfMarket?: Maybe<IdResponseOfMarket>;
};

export type DateTimeOperationFilterInput = {
  eq?: InputMaybe<Scalars['DateTime']['input']>;
  gt?: InputMaybe<Scalars['DateTime']['input']>;
  gte?: InputMaybe<Scalars['DateTime']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['DateTime']['input']>>>;
  lt?: InputMaybe<Scalars['DateTime']['input']>;
  lte?: InputMaybe<Scalars['DateTime']['input']>;
  neq?: InputMaybe<Scalars['DateTime']['input']>;
  ngt?: InputMaybe<Scalars['DateTime']['input']>;
  ngte?: InputMaybe<Scalars['DateTime']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['DateTime']['input']>>>;
  nlt?: InputMaybe<Scalars['DateTime']['input']>;
  nlte?: InputMaybe<Scalars['DateTime']['input']>;
};

export type DecimalOperationFilterInput = {
  eq?: InputMaybe<Scalars['Decimal']['input']>;
  gt?: InputMaybe<Scalars['Decimal']['input']>;
  gte?: InputMaybe<Scalars['Decimal']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['Decimal']['input']>>>;
  lt?: InputMaybe<Scalars['Decimal']['input']>;
  lte?: InputMaybe<Scalars['Decimal']['input']>;
  neq?: InputMaybe<Scalars['Decimal']['input']>;
  ngt?: InputMaybe<Scalars['Decimal']['input']>;
  ngte?: InputMaybe<Scalars['Decimal']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['Decimal']['input']>>>;
  nlt?: InputMaybe<Scalars['Decimal']['input']>;
  nlte?: InputMaybe<Scalars['Decimal']['input']>;
};

export type DeleteCharacterInput = {
  /** Id of the character to delete. */
  characterId: Scalars['String']['input'];
};

export type DeleteCharacterPayload = {
  __typename?: 'DeleteCharacterPayload';
  idResponseOfCharacter?: Maybe<IdResponseOfCharacter>;
};

export type DeleteMarketInput = {
  /** Id of the market to delete. */
  marketId: Scalars['String']['input'];
};

export type DeleteMarketPayload = {
  __typename?: 'DeleteMarketPayload';
  idResponseOfMarket?: Maybe<IdResponseOfMarket>;
};

export type FlatTax = {
  __typename?: 'FlatTax';
  maximum: Scalars['Decimal']['output'];
  minimum: Scalars['Decimal']['output'];
  rate: Scalars['Decimal']['output'];
};

export type FlatTaxFilterInput = {
  and?: InputMaybe<Array<FlatTaxFilterInput>>;
  maximum?: InputMaybe<DecimalOperationFilterInput>;
  minimum?: InputMaybe<DecimalOperationFilterInput>;
  or?: InputMaybe<Array<FlatTaxFilterInput>>;
  rate?: InputMaybe<DecimalOperationFilterInput>;
};

export type FlatTaxInput = {
  maximum: Scalars['Decimal']['input'];
  minimum: Scalars['Decimal']['input'];
  rate: Scalars['Decimal']['input'];
};

export type FlatTaxSortInput = {
  maximum?: InputMaybe<SortEnumType>;
  minimum?: InputMaybe<SortEnumType>;
  rate?: InputMaybe<SortEnumType>;
};

export type Forecast = {
  __typename?: 'Forecast';
  createdAt: Scalars['DateTime']['output'];
  id: Scalars['String']['output'];
  latest: ForecastPoint;
  marketId: Scalars['String']['output'];
  predictions: Array<ForecastPoint>;
  symbolId: Scalars['String']['output'];
  symbolName: Scalars['String']['output'];
  symbolSubcode?: Maybe<Scalars['String']['output']>;
  updatedAt: Scalars['DateTime']['output'];
};

export type ForecastFilterInput = {
  and?: InputMaybe<Array<ForecastFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  id?: InputMaybe<IdFilterInputTypeOfForecastFilterInput>;
  latest?: InputMaybe<ForecastPointFilterInput>;
  marketId?: InputMaybe<IdFilterInputTypeOfMarketFilterInput>;
  or?: InputMaybe<Array<ForecastFilterInput>>;
  predictions?: InputMaybe<ListFilterInputTypeOfForecastPointFilterInput>;
  symbolId?: InputMaybe<IdFilterInputTypeOfSymbolFilterInput>;
  symbolName?: InputMaybe<StringOperationFilterInput>;
  symbolSubcode?: InputMaybe<StringOperationFilterInput>;
  updatedAt?: InputMaybe<DateTimeOperationFilterInput>;
};

export type ForecastPoint = {
  __typename?: 'ForecastPoint';
  averagePrice: Scalars['Decimal']['output'];
  maxPrice: Scalars['Decimal']['output'];
  minPrice: Scalars['Decimal']['output'];
  volume: Scalars['Decimal']['output'];
};

export type ForecastPointFilterInput = {
  and?: InputMaybe<Array<ForecastPointFilterInput>>;
  averagePrice?: InputMaybe<DecimalOperationFilterInput>;
  maxPrice?: InputMaybe<DecimalOperationFilterInput>;
  minPrice?: InputMaybe<DecimalOperationFilterInput>;
  or?: InputMaybe<Array<ForecastPointFilterInput>>;
  volume?: InputMaybe<DecimalOperationFilterInput>;
};

export type ForecastPointSortInput = {
  averagePrice?: InputMaybe<SortEnumType>;
  maxPrice?: InputMaybe<SortEnumType>;
  minPrice?: InputMaybe<SortEnumType>;
  volume?: InputMaybe<SortEnumType>;
};

export type ForecastSortInput = {
  createdAt?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  latest?: InputMaybe<ForecastPointSortInput>;
  marketId?: InputMaybe<SortEnumType>;
  symbolId?: InputMaybe<SortEnumType>;
  symbolName?: InputMaybe<SortEnumType>;
  symbolSubcode?: InputMaybe<SortEnumType>;
  updatedAt?: InputMaybe<SortEnumType>;
};

export type GenerateCompletionInput = {
  conversation: ConversationInput;
};

export type GenerateCompletionPayload = {
  __typename?: 'GenerateCompletionPayload';
  completionResponse?: Maybe<CompletionResponse>;
};

export type GenerateCompletionResponse = {
  __typename?: 'GenerateCompletionResponse';
  content: Scalars['String']['output'];
};

export type GetDailySymbolSummaryInput = {
  symbolId: Scalars['String']['input'];
};

export type GetDailySymbolSummaryResponse = {
  __typename?: 'GetDailySymbolSummaryResponse';
  averagePrice: Scalars['Decimal']['output'];
  maxPrice: Scalars['Decimal']['output'];
  minPrice: Scalars['Decimal']['output'];
  volume: Scalars['Decimal']['output'];
};

export type GetMarketForecastInput = {
  marketId: Scalars['String']['input'];
};

export type GetMarketForecastPredictionResponse = {
  __typename?: 'GetMarketForecastPredictionResponse';
  averagePrice: Scalars['Decimal']['output'];
  averagePriceDelta: Scalars['Decimal']['output'];
  gain: Scalars['Decimal']['output'];
  gainDelta: Scalars['Decimal']['output'];
  margin: Scalars['Decimal']['output'];
  maxPrice: Scalars['Decimal']['output'];
  maxPriceDelta: Scalars['Decimal']['output'];
  minPrice: Scalars['Decimal']['output'];
  minPriceDelta: Scalars['Decimal']['output'];
  volume: Scalars['Decimal']['output'];
  volumeDelta: Scalars['Decimal']['output'];
};

export type GetMarketForecastPredictionResponseFilterInput = {
  and?: InputMaybe<Array<GetMarketForecastPredictionResponseFilterInput>>;
  averagePrice?: InputMaybe<DecimalOperationFilterInput>;
  averagePriceDelta?: InputMaybe<DecimalOperationFilterInput>;
  gain?: InputMaybe<DecimalOperationFilterInput>;
  gainDelta?: InputMaybe<DecimalOperationFilterInput>;
  margin?: InputMaybe<DecimalOperationFilterInput>;
  maxPrice?: InputMaybe<DecimalOperationFilterInput>;
  maxPriceDelta?: InputMaybe<DecimalOperationFilterInput>;
  minPrice?: InputMaybe<DecimalOperationFilterInput>;
  minPriceDelta?: InputMaybe<DecimalOperationFilterInput>;
  or?: InputMaybe<Array<GetMarketForecastPredictionResponseFilterInput>>;
  volume?: InputMaybe<DecimalOperationFilterInput>;
  volumeDelta?: InputMaybe<DecimalOperationFilterInput>;
};

export type GetMarketForecastPredictionResponseSortInput = {
  averagePrice?: InputMaybe<SortEnumType>;
  averagePriceDelta?: InputMaybe<SortEnumType>;
  gain?: InputMaybe<SortEnumType>;
  gainDelta?: InputMaybe<SortEnumType>;
  margin?: InputMaybe<SortEnumType>;
  maxPrice?: InputMaybe<SortEnumType>;
  maxPriceDelta?: InputMaybe<SortEnumType>;
  minPrice?: InputMaybe<SortEnumType>;
  minPriceDelta?: InputMaybe<SortEnumType>;
  volume?: InputMaybe<SortEnumType>;
  volumeDelta?: InputMaybe<SortEnumType>;
};

export type GetMarketForecastResponse = {
  __typename?: 'GetMarketForecastResponse';
  dayFive: GetMarketForecastPredictionResponse;
  dayFour: GetMarketForecastPredictionResponse;
  dayOne: GetMarketForecastPredictionResponse;
  daySeven: GetMarketForecastPredictionResponse;
  daySix: GetMarketForecastPredictionResponse;
  dayThree: GetMarketForecastPredictionResponse;
  dayTwo: GetMarketForecastPredictionResponse;
  id: Scalars['String']['output'];
  latest: ForecastPoint;
  marketId: Scalars['String']['output'];
  symbolId: Scalars['String']['output'];
  symbolName: Scalars['String']['output'];
  symbolSubcode?: Maybe<Scalars['String']['output']>;
};

export type GetMarketForecastResponseFilterInput = {
  and?: InputMaybe<Array<GetMarketForecastResponseFilterInput>>;
  dayFive?: InputMaybe<GetMarketForecastPredictionResponseFilterInput>;
  dayFour?: InputMaybe<GetMarketForecastPredictionResponseFilterInput>;
  dayOne?: InputMaybe<GetMarketForecastPredictionResponseFilterInput>;
  daySeven?: InputMaybe<GetMarketForecastPredictionResponseFilterInput>;
  daySix?: InputMaybe<GetMarketForecastPredictionResponseFilterInput>;
  dayThree?: InputMaybe<GetMarketForecastPredictionResponseFilterInput>;
  dayTwo?: InputMaybe<GetMarketForecastPredictionResponseFilterInput>;
  id?: InputMaybe<IdFilterInputTypeOfForecastFilterInput>;
  latest?: InputMaybe<ForecastPointFilterInput>;
  marketId?: InputMaybe<IdFilterInputTypeOfMarketFilterInput>;
  or?: InputMaybe<Array<GetMarketForecastResponseFilterInput>>;
  symbolId?: InputMaybe<IdFilterInputTypeOfSymbolFilterInput>;
  symbolName?: InputMaybe<StringOperationFilterInput>;
  symbolSubcode?: InputMaybe<StringOperationFilterInput>;
};

export type GetMarketForecastResponseSortInput = {
  dayFive?: InputMaybe<GetMarketForecastPredictionResponseSortInput>;
  dayFour?: InputMaybe<GetMarketForecastPredictionResponseSortInput>;
  dayOne?: InputMaybe<GetMarketForecastPredictionResponseSortInput>;
  daySeven?: InputMaybe<GetMarketForecastPredictionResponseSortInput>;
  daySix?: InputMaybe<GetMarketForecastPredictionResponseSortInput>;
  dayThree?: InputMaybe<GetMarketForecastPredictionResponseSortInput>;
  dayTwo?: InputMaybe<GetMarketForecastPredictionResponseSortInput>;
  id?: InputMaybe<SortEnumType>;
  latest?: InputMaybe<ForecastPointSortInput>;
  marketId?: InputMaybe<SortEnumType>;
  symbolId?: InputMaybe<SortEnumType>;
  symbolName?: InputMaybe<SortEnumType>;
  symbolSubcode?: InputMaybe<SortEnumType>;
};

export type GetMarketTradesInput = {
  marketId: Scalars['String']['input'];
  seconds?: InputMaybe<Scalars['Float']['input']>;
};

export type GetMarketTradesResponse = {
  __typename?: 'GetMarketTradesResponse';
  averagePrice: Scalars['Decimal']['output'];
  limit: Scalars['Decimal']['output'];
  margin: Scalars['Decimal']['output'];
  maxPrice: Scalars['Decimal']['output'];
  minPrice: Scalars['Decimal']['output'];
  numTransactions: Scalars['Int']['output'];
  roi: Scalars['Decimal']['output'];
  symbolCode: Scalars['String']['output'];
  symbolId: Scalars['String']['output'];
  symbolName: Scalars['String']['output'];
  symbolSubcode?: Maybe<Scalars['String']['output']>;
  totalGain: Scalars['Decimal']['output'];
  totalSpent: Scalars['Decimal']['output'];
  totalVolume: Scalars['Decimal']['output'];
};

export type GetMarketTradesResponseFilterInput = {
  and?: InputMaybe<Array<GetMarketTradesResponseFilterInput>>;
  averagePrice?: InputMaybe<DecimalOperationFilterInput>;
  limit?: InputMaybe<DecimalOperationFilterInput>;
  margin?: InputMaybe<DecimalOperationFilterInput>;
  maxPrice?: InputMaybe<DecimalOperationFilterInput>;
  minPrice?: InputMaybe<DecimalOperationFilterInput>;
  numTransactions?: InputMaybe<IntOperationFilterInput>;
  or?: InputMaybe<Array<GetMarketTradesResponseFilterInput>>;
  roi?: InputMaybe<DecimalOperationFilterInput>;
  symbolCode?: InputMaybe<StringOperationFilterInput>;
  symbolId?: InputMaybe<IdFilterInputTypeOfSymbolFilterInput>;
  symbolName?: InputMaybe<StringOperationFilterInput>;
  symbolSubcode?: InputMaybe<StringOperationFilterInput>;
  totalGain?: InputMaybe<DecimalOperationFilterInput>;
  totalSpent?: InputMaybe<DecimalOperationFilterInput>;
  totalVolume?: InputMaybe<DecimalOperationFilterInput>;
};

export type GetMarketTradesResponseSortInput = {
  averagePrice?: InputMaybe<SortEnumType>;
  limit?: InputMaybe<SortEnumType>;
  margin?: InputMaybe<SortEnumType>;
  maxPrice?: InputMaybe<SortEnumType>;
  minPrice?: InputMaybe<SortEnumType>;
  numTransactions?: InputMaybe<SortEnumType>;
  roi?: InputMaybe<SortEnumType>;
  symbolCode?: InputMaybe<SortEnumType>;
  symbolId?: InputMaybe<SortEnumType>;
  symbolName?: InputMaybe<SortEnumType>;
  symbolSubcode?: InputMaybe<SortEnumType>;
  totalGain?: InputMaybe<SortEnumType>;
  totalSpent?: InputMaybe<SortEnumType>;
  totalVolume?: InputMaybe<SortEnumType>;
};

export type GetRecipeTradesInput = {
  marketId: Scalars['String']['input'];
  seconds?: InputMaybe<Scalars['Float']['input']>;
};

export type GetRecipeTradesResponse = {
  __typename?: 'GetRecipeTradesResponse';
  averageBuyPrice: Scalars['Decimal']['output'];
  averageMargin: Scalars['Decimal']['output'];
  averageSellPrice: Scalars['Decimal']['output'];
  latestBuyPrice: Scalars['Decimal']['output'];
  latestMargin: Scalars['Decimal']['output'];
  latestSellPrice: Scalars['Decimal']['output'];
  recipeId: Scalars['String']['output'];
  recipeName: Scalars['String']['output'];
};

export type GetRecipeTradesResponseFilterInput = {
  and?: InputMaybe<Array<GetRecipeTradesResponseFilterInput>>;
  averageBuyPrice?: InputMaybe<DecimalOperationFilterInput>;
  averageMargin?: InputMaybe<DecimalOperationFilterInput>;
  averageSellPrice?: InputMaybe<DecimalOperationFilterInput>;
  latestBuyPrice?: InputMaybe<DecimalOperationFilterInput>;
  latestMargin?: InputMaybe<DecimalOperationFilterInput>;
  latestSellPrice?: InputMaybe<DecimalOperationFilterInput>;
  or?: InputMaybe<Array<GetRecipeTradesResponseFilterInput>>;
  recipeId?: InputMaybe<IdFilterInputTypeOfRecipeFilterInput>;
  recipeName?: InputMaybe<StringOperationFilterInput>;
};

export type GetRecipeTradesResponseSortInput = {
  averageBuyPrice?: InputMaybe<SortEnumType>;
  averageMargin?: InputMaybe<SortEnumType>;
  averageSellPrice?: InputMaybe<SortEnumType>;
  latestBuyPrice?: InputMaybe<SortEnumType>;
  latestMargin?: InputMaybe<SortEnumType>;
  latestSellPrice?: InputMaybe<SortEnumType>;
  recipeId?: InputMaybe<SortEnumType>;
  recipeName?: InputMaybe<SortEnumType>;
};

export type GetSymbolTradeBucketsResponse = {
  __typename?: 'GetSymbolTradeBucketsResponse';
  date: Scalars['DateTime']['output'];
  maxPrice: Scalars['Decimal']['output'];
  minPrice: Scalars['Decimal']['output'];
  numTransactions: Scalars['Int']['output'];
  price: Scalars['Decimal']['output'];
  totalSpent: Scalars['Decimal']['output'];
  volume: Scalars['Decimal']['output'];
};

export type GetSymbolTradesInput = {
  numBuckets?: Scalars['Int']['input'];
  seconds?: InputMaybe<Scalars['Float']['input']>;
  symbolId: Scalars['String']['input'];
};

export type GetSymbolTradesResponse = {
  __typename?: 'GetSymbolTradesResponse';
  averagePrice: Scalars['Decimal']['output'];
  maxPrice: Scalars['Decimal']['output'];
  minPrice: Scalars['Decimal']['output'];
  numTransactions: Scalars['Int']['output'];
  totalSpent: Scalars['Decimal']['output'];
  trades: Array<GetSymbolTradeBucketsResponse>;
  volume: Scalars['Decimal']['output'];
};

export type IdFilterInputTypeOfCharacterFilterInput = {
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<Scalars['String']['input']>>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<Scalars['String']['input']>>;
};

export type IdFilterInputTypeOfForecastFilterInput = {
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<Scalars['String']['input']>>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<Scalars['String']['input']>>;
};

export type IdFilterInputTypeOfMarketFilterInput = {
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<Scalars['String']['input']>>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<Scalars['String']['input']>>;
};

export type IdFilterInputTypeOfRecipeFilterInput = {
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<Scalars['String']['input']>>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<Scalars['String']['input']>>;
};

export type IdFilterInputTypeOfSymbolFilterInput = {
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<Scalars['String']['input']>>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<Scalars['String']['input']>>;
};

export type IdFilterInputTypeOfSymbolGroupFilterInput = {
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<Scalars['String']['input']>>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<Scalars['String']['input']>>;
};

export type IdFilterInputTypeOfTradeFilterInput = {
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<Scalars['String']['input']>>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<Scalars['String']['input']>>;
};

export type IdResponseOfCharacter = {
  __typename?: 'IdResponseOfCharacter';
  id: Scalars['String']['output'];
};

export type IdResponseOfMarket = {
  __typename?: 'IdResponseOfMarket';
  id: Scalars['String']['output'];
};

export type IdResponseOfRecipe = {
  __typename?: 'IdResponseOfRecipe';
  id: Scalars['String']['output'];
};

export type IntOperationFilterInput = {
  eq?: InputMaybe<Scalars['Int']['input']>;
  gt?: InputMaybe<Scalars['Int']['input']>;
  gte?: InputMaybe<Scalars['Int']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['Int']['input']>>>;
  lt?: InputMaybe<Scalars['Int']['input']>;
  lte?: InputMaybe<Scalars['Int']['input']>;
  neq?: InputMaybe<Scalars['Int']['input']>;
  ngt?: InputMaybe<Scalars['Int']['input']>;
  ngte?: InputMaybe<Scalars['Int']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['Int']['input']>>>;
  nlt?: InputMaybe<Scalars['Int']['input']>;
  nlte?: InputMaybe<Scalars['Int']['input']>;
};

export type ListFilterInputTypeOfCharacterDetailFilterInput = {
  all?: InputMaybe<CharacterDetailFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<CharacterDetailFilterInput>;
  some?: InputMaybe<CharacterDetailFilterInput>;
};

export type ListFilterInputTypeOfForecastPointFilterInput = {
  all?: InputMaybe<ForecastPointFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<ForecastPointFilterInput>;
  some?: InputMaybe<ForecastPointFilterInput>;
};

export type ListFilterInputTypeOfRecipeComponentFilterInput = {
  all?: InputMaybe<RecipeComponentFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<RecipeComponentFilterInput>;
  some?: InputMaybe<RecipeComponentFilterInput>;
};

export type ListIdFilterInputTypeOfSymbolFilterInput = {
  all?: InputMaybe<IdFilterInputTypeOfSymbolFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<IdFilterInputTypeOfSymbolFilterInput>;
  some?: InputMaybe<IdFilterInputTypeOfSymbolFilterInput>;
};

export type Market = {
  __typename?: 'Market';
  createdAt: Scalars['DateTime']['output'];
  description?: Maybe<Scalars['String']['output']>;
  icon?: Maybe<Scalars['String']['output']>;
  id: Scalars['String']['output'];
  isForecastingEnabled: Scalars['Boolean']['output'];
  name: Scalars['String']['output'];
  taxes: Taxes;
  updatedAt: Scalars['DateTime']['output'];
};

export type MarketFilterInput = {
  and?: InputMaybe<Array<MarketFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  description?: InputMaybe<StringOperationFilterInput>;
  icon?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<IdFilterInputTypeOfMarketFilterInput>;
  isForecastingEnabled?: InputMaybe<BooleanOperationFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<MarketFilterInput>>;
  taxes?: InputMaybe<TaxesFilterInput>;
  updatedAt?: InputMaybe<DateTimeOperationFilterInput>;
};

/** A connection to a list of items. */
export type MarketForecastConnection = {
  __typename?: 'MarketForecastConnection';
  /** A list of edges. */
  edges?: Maybe<Array<MarketForecastEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<GetMarketForecastResponse>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type MarketForecastEdge = {
  __typename?: 'MarketForecastEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: GetMarketForecastResponse;
};

export type MarketSortInput = {
  createdAt?: InputMaybe<SortEnumType>;
  description?: InputMaybe<SortEnumType>;
  icon?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isForecastingEnabled?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
  taxes?: InputMaybe<TaxesSortInput>;
  updatedAt?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type MarketTradesConnection = {
  __typename?: 'MarketTradesConnection';
  /** A list of edges. */
  edges?: Maybe<Array<MarketTradesEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<GetMarketTradesResponse>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type MarketTradesEdge = {
  __typename?: 'MarketTradesEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: GetMarketTradesResponse;
};

export type MessageInput = {
  content: Scalars['String']['input'];
  role: Role;
};

export type Mutation = {
  __typename?: 'Mutation';
  /**
   * Creates a character.
   *
   *
   * **Returns:**
   * The id of the newly created character.
   */
  createCharacter: CreateCharacterPayload;
  /**
   * Creates a market.
   *
   *
   * **Returns:**
   * Response containing the created market's identifier.
   */
  createMarket: CreateMarketPayload;
  /**
   * Deletes a character.
   *
   *
   * **Returns:**
   * The id of the recently deleted character.
   */
  deleteCharacter: DeleteCharacterPayload;
  /**
   * Deletes a market by it's identifier.
   *
   *
   * **Returns:**
   * Status code.
   */
  deleteMarket: DeleteMarketPayload;
  /**
   * Generates a completion given some conversation input.
   *
   *
   * **Returns:**
   * Generated completion response.
   */
  generateCompletion: GenerateCompletionPayload;
  /**
   * Updates a character.
   *
   *
   * **Returns:**
   * The id of the updated character.
   */
  updateCharacter: UpdateCharacterPayload;
  /**
   * Updates a market by it's identifier.
   *
   *
   * **Returns:**
   * Status code.
   */
  updateMarket: UpdateMarketPayload;
  /**
   * Updates a recipe.
   *
   *
   * **Returns:**
   * The id of the updated recipe.
   */
  updateRecipe: UpdateRecipePayload;
};


export type MutationCreateCharacterArgs = {
  input: CreateCharacterInput;
};


export type MutationCreateMarketArgs = {
  input: CreateMarketInput;
};


export type MutationDeleteCharacterArgs = {
  input: DeleteCharacterInput;
};


export type MutationDeleteMarketArgs = {
  input: DeleteMarketInput;
};


export type MutationGenerateCompletionArgs = {
  input: GenerateCompletionInput;
};


export type MutationUpdateCharacterArgs = {
  input: UpdateCharacterInput;
};


export type MutationUpdateMarketArgs = {
  input: UpdateMarketInput;
};


export type MutationUpdateRecipeArgs = {
  input: UpdateRecipeInput;
};

/** Information about pagination in a connection. */
export type PageInfo = {
  __typename?: 'PageInfo';
  /** When paginating forwards, the cursor to continue. */
  endCursor?: Maybe<Scalars['String']['output']>;
  /** Indicates whether more edges exist following the set defined by the clients arguments. */
  hasNextPage: Scalars['Boolean']['output'];
  /** Indicates whether more edges exist prior the set defined by the clients arguments. */
  hasPreviousPage: Scalars['Boolean']['output'];
  /** When paginating backwards, the cursor to continue. */
  startCursor?: Maybe<Scalars['String']['output']>;
};

export type Query = {
  __typename?: 'Query';
  /**
   * Gets a queryable list of characters.
   *
   *
   * **Returns:**
   * A list of characters.
   */
  allCharacters: Array<Character>;
  /**
   * Gets all forecasts.
   *
   *
   * **Returns:**
   * List of all forecasts.
   */
  allForecasts?: Maybe<AllForecastsConnection>;
  /**
   * Gets all markets.
   *
   *
   * **Returns:**
   * List of all markets.
   */
  allMarkets?: Maybe<AllMarketsConnection>;
  /**
   * Gets all recipes.
   *
   *
   * **Returns:**
   * List of all recipes.
   */
  allRecipes?: Maybe<AllRecipesConnection>;
  /**
   * Gets all symbol groups.
   *
   *
   * **Returns:**
   * List of all symbol groups.
   */
  allSymbolGroups?: Maybe<AllSymbolGroupsConnection>;
  /**
   * Gets all symbols.
   *
   *
   * **Returns:**
   * Queryable list of all symbols.
   */
  allSymbols?: Maybe<AllSymbolsConnection>;
  /**
   * Gets all trades.
   *
   *
   * **Returns:**
   * List of all trades.
   */
  allTrades?: Maybe<AllTradesConnection>;
  /**
   * Gets a character.
   *
   *
   * **Returns:**
   * The character matching the given query.
   */
  character: Character;
  /**
   * Gets the daily summary of trades for a symbol.
   *
   *
   * **Returns:**
   * Summary of trades for the given symbol for today.
   */
  dailySymbolSummary: GetDailySymbolSummaryResponse;
  /**
   * Retrieves a market by it's identifier.
   *
   *
   * **Returns:**
   * The market matching the given query.
   */
  market: Market;
  /**
   * Gets all forecasts.
   *
   *
   * **Returns:**
   * List of all forecasts.
   */
  marketForecast?: Maybe<MarketForecastConnection>;
  /**
   * Retrieves information about the symbols in a market.
   *
   *
   * **Returns:**
   * Trade statistics for the symbols in a market.
   */
  marketTrades?: Maybe<MarketTradesConnection>;
  /**
   * Retrieves a recipe by its identifier.
   *
   *
   * **Returns:**
   * The recipe matching the given query.
   */
  recipe: Recipe;
  /**
   * Gets recipe trade information.
   *
   *
   * **Returns:**
   * List of recipe trade information.
   */
  recipeTrades?: Maybe<RecipeTradesConnection>;
  /**
   * Gets a symbol by its identifier.
   *
   *
   * **Returns:**
   * The symbol matching the given query.
   */
  symbol: Symbol;
  /**
   * Retrieves a symbol group by its identifier.
   *
   *
   * **Returns:**
   * The symbol group matching the given query.
   */
  symbolGroup: SymbolGroup;
  /**
   * Retrieves information about the trades for a given symbol.
   *
   *
   * **Returns:**
   * Trade statistics for a symbol.
   */
  symbolTrades: GetSymbolTradesResponse;
};


export type QueryAllCharactersArgs = {
  order?: InputMaybe<Array<CharacterSortInput>>;
  where?: InputMaybe<CharacterFilterInput>;
};


export type QueryAllForecastsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<ForecastSortInput>>;
  where?: InputMaybe<ForecastFilterInput>;
};


export type QueryAllMarketsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<MarketSortInput>>;
  where?: InputMaybe<MarketFilterInput>;
};


export type QueryAllRecipesArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<RecipeSortInput>>;
  where?: InputMaybe<RecipeFilterInput>;
};


export type QueryAllSymbolGroupsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<SymbolGroupSortInput>>;
  where?: InputMaybe<SymbolGroupFilterInput>;
};


export type QueryAllSymbolsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<SymbolSortInput>>;
  where?: InputMaybe<SymbolFilterInput>;
};


export type QueryAllTradesArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<TradeSortInput>>;
  where?: InputMaybe<TradeFilterInput>;
};


export type QueryCharacterArgs = {
  characterId: Scalars['String']['input'];
};


export type QueryDailySymbolSummaryArgs = {
  input: GetDailySymbolSummaryInput;
};


export type QueryMarketArgs = {
  marketId: Scalars['String']['input'];
};


export type QueryMarketForecastArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  input: GetMarketForecastInput;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<GetMarketForecastResponseSortInput>>;
  where?: InputMaybe<GetMarketForecastResponseFilterInput>;
};


export type QueryMarketTradesArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  input: GetMarketTradesInput;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<GetMarketTradesResponseSortInput>>;
  where?: InputMaybe<GetMarketTradesResponseFilterInput>;
};


export type QueryRecipeArgs = {
  recipeId: Scalars['String']['input'];
};


export type QueryRecipeTradesArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  input: GetRecipeTradesInput;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<GetRecipeTradesResponseSortInput>>;
  where?: InputMaybe<GetRecipeTradesResponseFilterInput>;
};


export type QuerySymbolArgs = {
  symbolId: Scalars['String']['input'];
};


export type QuerySymbolGroupArgs = {
  symbolGroupId: Scalars['String']['input'];
};


export type QuerySymbolTradesArgs = {
  input: GetSymbolTradesInput;
};

export type Recipe = {
  __typename?: 'Recipe';
  cost: Scalars['Decimal']['output'];
  createdAt: Scalars['DateTime']['output'];
  id: Scalars['String']['output'];
  inputs: Array<RecipeComponent>;
  marketId: Scalars['String']['output'];
  name: Scalars['String']['output'];
  outputs: Array<RecipeComponent>;
  updatedAt: Scalars['DateTime']['output'];
};

export type RecipeComponent = {
  __typename?: 'RecipeComponent';
  name: Scalars['String']['output'];
  quantity: Scalars['Int']['output'];
  symbolId: Scalars['String']['output'];
};

export type RecipeComponentFilterInput = {
  and?: InputMaybe<Array<RecipeComponentFilterInput>>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<RecipeComponentFilterInput>>;
  quantity?: InputMaybe<IntOperationFilterInput>;
  symbolId?: InputMaybe<IdFilterInputTypeOfSymbolFilterInput>;
};

export type RecipeComponentInput = {
  name: Scalars['String']['input'];
  quantity: Scalars['Int']['input'];
  symbolId: Scalars['String']['input'];
};

export type RecipeFilterInput = {
  and?: InputMaybe<Array<RecipeFilterInput>>;
  cost?: InputMaybe<DecimalOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  id?: InputMaybe<IdFilterInputTypeOfRecipeFilterInput>;
  inputs?: InputMaybe<ListFilterInputTypeOfRecipeComponentFilterInput>;
  marketId?: InputMaybe<IdFilterInputTypeOfMarketFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<RecipeFilterInput>>;
  outputs?: InputMaybe<ListFilterInputTypeOfRecipeComponentFilterInput>;
  updatedAt?: InputMaybe<DateTimeOperationFilterInput>;
};

export type RecipeSortInput = {
  cost?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  marketId?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
  updatedAt?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type RecipeTradesConnection = {
  __typename?: 'RecipeTradesConnection';
  /** A list of edges. */
  edges?: Maybe<Array<RecipeTradesEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<GetRecipeTradesResponse>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type RecipeTradesEdge = {
  __typename?: 'RecipeTradesEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: GetRecipeTradesResponse;
};

export enum Role {
  Assistant = 'ASSISTANT',
  System = 'SYSTEM',
  User = 'USER'
}

export enum SortEnumType {
  Asc = 'ASC',
  Desc = 'DESC'
}

export type StringOperationFilterInput = {
  and?: InputMaybe<Array<StringOperationFilterInput>>;
  contains?: InputMaybe<Scalars['String']['input']>;
  endsWith?: InputMaybe<Scalars['String']['input']>;
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['String']['input']>>>;
  ncontains?: InputMaybe<Scalars['String']['input']>;
  nendsWith?: InputMaybe<Scalars['String']['input']>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['String']['input']>>>;
  nstartsWith?: InputMaybe<Scalars['String']['input']>;
  or?: InputMaybe<Array<StringOperationFilterInput>>;
  startsWith?: InputMaybe<Scalars['String']['input']>;
};

export type Symbol = {
  __typename?: 'Symbol';
  additionalFields: AdditionalFields;
  code: Scalars['String']['output'];
  createdAt: Scalars['DateTime']['output'];
  id: Scalars['String']['output'];
  marketId: Scalars['String']['output'];
  name: Scalars['String']['output'];
  subcode?: Maybe<Scalars['String']['output']>;
  updatedAt: Scalars['DateTime']['output'];
};

export type SymbolFilterInput = {
  additionalFields?: InputMaybe<AdditionalFieldsFilterInput>;
  and?: InputMaybe<Array<SymbolFilterInput>>;
  code?: InputMaybe<StringOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  id?: InputMaybe<IdFilterInputTypeOfSymbolFilterInput>;
  marketId?: InputMaybe<IdFilterInputTypeOfMarketFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<SymbolFilterInput>>;
  subcode?: InputMaybe<StringOperationFilterInput>;
  updatedAt?: InputMaybe<DateTimeOperationFilterInput>;
};

export type SymbolGroup = {
  __typename?: 'SymbolGroup';
  createdAt: Scalars['DateTime']['output'];
  id: Scalars['String']['output'];
  marketId: Scalars['String']['output'];
  name: Scalars['String']['output'];
  symbolIds: Array<Scalars['String']['output']>;
  updatedAt: Scalars['DateTime']['output'];
};

export type SymbolGroupFilterInput = {
  and?: InputMaybe<Array<SymbolGroupFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  id?: InputMaybe<IdFilterInputTypeOfSymbolGroupFilterInput>;
  marketId?: InputMaybe<IdFilterInputTypeOfMarketFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<SymbolGroupFilterInput>>;
  symbolIds?: InputMaybe<ListIdFilterInputTypeOfSymbolFilterInput>;
  updatedAt?: InputMaybe<DateTimeOperationFilterInput>;
};

export type SymbolGroupSortInput = {
  createdAt?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  marketId?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
  updatedAt?: InputMaybe<SortEnumType>;
};

export type SymbolSortInput = {
  additionalFields?: InputMaybe<AdditionalFieldsSortInput>;
  code?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  marketId?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
  subcode?: InputMaybe<SortEnumType>;
  updatedAt?: InputMaybe<SortEnumType>;
};

export type Taxes = {
  __typename?: 'Taxes';
  flat?: Maybe<FlatTax>;
};

export type TaxesFilterInput = {
  and?: InputMaybe<Array<TaxesFilterInput>>;
  flat?: InputMaybe<FlatTaxFilterInput>;
  or?: InputMaybe<Array<TaxesFilterInput>>;
};

export type TaxesInput = {
  flat?: InputMaybe<FlatTaxInput>;
};

export type TaxesSortInput = {
  flat?: InputMaybe<FlatTaxSortInput>;
};

export type Trade = {
  __typename?: 'Trade';
  createdAt: Scalars['DateTime']['output'];
  id: Scalars['String']['output'];
  metadata: TradeMetadata;
  price: Scalars['Decimal']['output'];
  updatedAt: Scalars['DateTime']['output'];
  volume: Scalars['Decimal']['output'];
};

export type TradeFilterInput = {
  and?: InputMaybe<Array<TradeFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  id?: InputMaybe<IdFilterInputTypeOfTradeFilterInput>;
  metadata?: InputMaybe<TradeMetadataFilterInput>;
  or?: InputMaybe<Array<TradeFilterInput>>;
  price?: InputMaybe<DecimalOperationFilterInput>;
  updatedAt?: InputMaybe<DateTimeOperationFilterInput>;
  volume?: InputMaybe<DecimalOperationFilterInput>;
};

export type TradeMetadata = {
  __typename?: 'TradeMetadata';
  additionalFields: AdditionalFields;
  marketId: Scalars['String']['output'];
  symbolCode: Scalars['String']['output'];
  symbolId: Scalars['String']['output'];
  symbolName: Scalars['String']['output'];
  symbolSubcode?: Maybe<Scalars['String']['output']>;
};

export type TradeMetadataFilterInput = {
  additionalFields?: InputMaybe<AdditionalFieldsFilterInput>;
  and?: InputMaybe<Array<TradeMetadataFilterInput>>;
  marketId?: InputMaybe<IdFilterInputTypeOfMarketFilterInput>;
  or?: InputMaybe<Array<TradeMetadataFilterInput>>;
  symbolCode?: InputMaybe<StringOperationFilterInput>;
  symbolId?: InputMaybe<IdFilterInputTypeOfSymbolFilterInput>;
  symbolName?: InputMaybe<StringOperationFilterInput>;
  symbolSubcode?: InputMaybe<StringOperationFilterInput>;
};

export type TradeMetadataSortInput = {
  additionalFields?: InputMaybe<AdditionalFieldsSortInput>;
  marketId?: InputMaybe<SortEnumType>;
  symbolCode?: InputMaybe<SortEnumType>;
  symbolId?: InputMaybe<SortEnumType>;
  symbolName?: InputMaybe<SortEnumType>;
  symbolSubcode?: InputMaybe<SortEnumType>;
};

export type TradeSortInput = {
  createdAt?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  metadata?: InputMaybe<TradeMetadataSortInput>;
  price?: InputMaybe<SortEnumType>;
  updatedAt?: InputMaybe<SortEnumType>;
  volume?: InputMaybe<SortEnumType>;
};

export type UpdateCharacterInput = {
  age: Scalars['Int']['input'];
  characterId: Scalars['String']['input'];
  details: Array<CharacterDetailInput>;
  name: Scalars['String']['input'];
};

export type UpdateCharacterPayload = {
  __typename?: 'UpdateCharacterPayload';
  idResponseOfCharacter?: Maybe<IdResponseOfCharacter>;
};

export type UpdateMarketInput = {
  marketId: Scalars['String']['input'];
  name: Scalars['String']['input'];
  taxes: TaxesInput;
};

export type UpdateMarketPayload = {
  __typename?: 'UpdateMarketPayload';
  idResponseOfMarket?: Maybe<IdResponseOfMarket>;
};

export type UpdateRecipeInput = {
  cost: Scalars['Decimal']['input'];
  inputs: Array<RecipeComponentInput>;
  marketId: Scalars['String']['input'];
  name: Scalars['String']['input'];
  outputs: Array<RecipeComponentInput>;
  recipeId: Scalars['String']['input'];
};

export type UpdateRecipePayload = {
  __typename?: 'UpdateRecipePayload';
  idResponseOfRecipe?: Maybe<IdResponseOfRecipe>;
};

export type DeleteCharacterMutationVariables = Exact<{
  input: DeleteCharacterInput;
}>;


export type DeleteCharacterMutation = { __typename?: 'Mutation', deleteCharacter: { __typename?: 'DeleteCharacterPayload', idResponseOfCharacter?: { __typename?: 'IdResponseOfCharacter', id: string } | null } };

export type CreateCharacterMutationVariables = Exact<{
  input: CreateCharacterInput;
}>;


export type CreateCharacterMutation = { __typename?: 'Mutation', createCharacter: { __typename?: 'CreateCharacterPayload', idResponseOfCharacter?: { __typename?: 'IdResponseOfCharacter', id: string } | null } };

export type UpdateCharacterMutationVariables = Exact<{
  input: UpdateCharacterInput;
}>;


export type UpdateCharacterMutation = { __typename?: 'Mutation', updateCharacter: { __typename?: 'UpdateCharacterPayload', idResponseOfCharacter?: { __typename?: 'IdResponseOfCharacter', id: string } | null } };

export type GenerateCompletionMutationVariables = Exact<{
  input: GenerateCompletionInput;
}>;


export type GenerateCompletionMutation = { __typename?: 'Mutation', generateCompletion: { __typename?: 'GenerateCompletionPayload', completionResponse?: { __typename?: 'CompletionResponse', chunks: Array<{ __typename?: 'GenerateCompletionResponse', content: string }> } | null } };

export type CharacterListQueryVariables = Exact<{ [key: string]: never; }>;


export type CharacterListQuery = { __typename?: 'Query', allCharacters: Array<{ __typename?: 'Character', id: string, name: string, age: number }> };

export type DetailedCharacterListQueryVariables = Exact<{ [key: string]: never; }>;


export type DetailedCharacterListQuery = { __typename?: 'Query', allCharacters: Array<{ __typename?: 'Character', id: string, name: string, age: number, details: Array<{ __typename?: 'CharacterDetail', key: string, value: string }> }> };

export type GetCharacterQueryVariables = Exact<{
  characterId: Scalars['String']['input'];
}>;


export type GetCharacterQuery = { __typename?: 'Query', character: { __typename?: 'Character', id: string, name: string, age: number, details: Array<{ __typename?: 'CharacterDetail', key: string, value: string }> } };

export type UpdateRecipeMutationVariables = Exact<{
  input: UpdateRecipeInput;
}>;


export type UpdateRecipeMutation = { __typename?: 'Mutation', updateRecipe: { __typename?: 'UpdateRecipePayload', idResponseOfRecipe?: { __typename?: 'IdResponseOfRecipe', id: string } | null } };

export type GetMarketsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetMarketsQuery = { __typename?: 'Query', allMarkets?: { __typename?: 'AllMarketsConnection', nodes?: Array<{ __typename?: 'Market', id: string, name: string, description?: string | null, icon?: string | null }> | null } | null };

export type GetMarketTradesQueryVariables = Exact<{
  input: GetMarketTradesInput;
  where?: InputMaybe<GetMarketTradesResponseFilterInput>;
  order?: InputMaybe<Array<GetMarketTradesResponseSortInput> | GetMarketTradesResponseSortInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
  after?: InputMaybe<Scalars['String']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
}>;


export type GetMarketTradesQuery = { __typename?: 'Query', marketTrades?: { __typename?: 'MarketTradesConnection', totalCount: number, nodes?: Array<{ __typename?: 'GetMarketTradesResponse', averagePrice: any, limit: any, margin: any, maxPrice: any, minPrice: any, numTransactions: number, roi: any, symbolCode: string, symbolId: string, symbolName: string, symbolSubcode?: string | null, totalGain: any, totalSpent: any, totalVolume: any }> | null, pageInfo: { __typename?: 'PageInfo', endCursor?: string | null, hasNextPage: boolean, hasPreviousPage: boolean, startCursor?: string | null } } | null };

export type GetRecentMarketTradesQueryVariables = Exact<{
  marketId: Scalars['String']['input'];
  first: Scalars['Int']['input'];
}>;


export type GetRecentMarketTradesQuery = { __typename?: 'Query', allTrades?: { __typename?: 'AllTradesConnection', nodes?: Array<{ __typename?: 'Trade', createdAt: any, price: any, volume: any, metadata: { __typename?: 'TradeMetadata', symbolId: string, symbolName: string, symbolSubcode?: string | null } }> | null } | null };

export type GetSymbolDetailsQueryVariables = Exact<{
  symbolId: Scalars['String']['input'];
  seconds?: InputMaybe<Scalars['Float']['input']>;
}>;


export type GetSymbolDetailsQuery = { __typename?: 'Query', symbol: { __typename?: 'Symbol', code: string, createdAt: any, id: string, marketId: string, name: string, subcode?: string | null, updatedAt: any }, symbolTrades: { __typename?: 'GetSymbolTradesResponse', totalSpent: any, averagePrice: any, minPrice: any, maxPrice: any, volume: any, numTransactions: number, trades: Array<{ __typename?: 'GetSymbolTradeBucketsResponse', date: any, maxPrice: any, minPrice: any, numTransactions: number, price: any, totalSpent: any, volume: any }> }, latestTrade?: { __typename?: 'AllTradesConnection', nodes?: Array<{ __typename?: 'Trade', price: any, volume: any }> | null } | null, allForecasts?: { __typename?: 'AllForecastsConnection', nodes?: Array<{ __typename?: 'Forecast', predictions: Array<{ __typename?: 'ForecastPoint', averagePrice: any }>, latest: { __typename?: 'ForecastPoint', averagePrice: any } }> | null } | null, dailySymbolSummary: { __typename?: 'GetDailySymbolSummaryResponse', averagePrice: any, maxPrice: any, minPrice: any, volume: any } };

export type GetRecipeTradesQueryVariables = Exact<{
  input: GetRecipeTradesInput;
  where?: InputMaybe<GetRecipeTradesResponseFilterInput>;
  order?: InputMaybe<Array<GetRecipeTradesResponseSortInput> | GetRecipeTradesResponseSortInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
  after?: InputMaybe<Scalars['String']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
}>;


export type GetRecipeTradesQuery = { __typename?: 'Query', recipeTrades?: { __typename?: 'RecipeTradesConnection', totalCount: number, nodes?: Array<{ __typename?: 'GetRecipeTradesResponse', averageBuyPrice: any, averageMargin: any, averageSellPrice: any, latestBuyPrice: any, latestMargin: any, latestSellPrice: any, recipeId: string, recipeName: string }> | null, pageInfo: { __typename?: 'PageInfo', endCursor?: string | null, hasNextPage: boolean, hasPreviousPage: boolean, startCursor?: string | null } } | null };

export type GetMarketForecastQueryVariables = Exact<{
  marketId: Scalars['String']['input'];
  where?: InputMaybe<GetMarketForecastResponseFilterInput>;
  order?: InputMaybe<Array<GetMarketForecastResponseSortInput> | GetMarketForecastResponseSortInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
  after?: InputMaybe<Scalars['String']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
}>;


export type GetMarketForecastQuery = { __typename?: 'Query', marketForecast?: { __typename?: 'MarketForecastConnection', totalCount: number, nodes?: Array<{ __typename?: 'GetMarketForecastResponse', id: string, symbolId: string, symbolName: string, symbolSubcode?: string | null, latest: { __typename?: 'ForecastPoint', averagePrice: any }, dayOne: { __typename?: 'GetMarketForecastPredictionResponse', averagePrice: any, margin: any, gain: any }, dayTwo: { __typename?: 'GetMarketForecastPredictionResponse', averagePrice: any, margin: any, gain: any } }> | null, pageInfo: { __typename?: 'PageInfo', endCursor?: string | null, hasNextPage: boolean, hasPreviousPage: boolean, startCursor?: string | null } } | null };

export type GetRecipeDetailsQueryVariables = Exact<{
  recipeId: Scalars['String']['input'];
}>;


export type GetRecipeDetailsQuery = { __typename?: 'Query', recipe: { __typename?: 'Recipe', id: string, name: string, cost: any, inputs: Array<{ __typename?: 'RecipeComponent', name: string, quantity: number, symbolId: string }>, outputs: Array<{ __typename?: 'RecipeComponent', name: string, quantity: number, symbolId: string }> } };

export type SearchSymbolsQueryVariables = Exact<{
  query: Scalars['String']['input'];
}>;


export type SearchSymbolsQuery = { __typename?: 'Query', allSymbols?: { __typename?: 'AllSymbolsConnection', nodes?: Array<{ __typename?: 'Symbol', id: string, name: string, code: string, subcode?: string | null }> | null } | null };


export const DeleteCharacterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"deleteCharacter"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DeleteCharacterInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteCharacter"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"idResponseOfCharacter"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<DeleteCharacterMutation, DeleteCharacterMutationVariables>;
export const CreateCharacterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"createCharacter"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateCharacterInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createCharacter"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"idResponseOfCharacter"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<CreateCharacterMutation, CreateCharacterMutationVariables>;
export const UpdateCharacterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"updateCharacter"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateCharacterInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateCharacter"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"idResponseOfCharacter"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<UpdateCharacterMutation, UpdateCharacterMutationVariables>;
export const GenerateCompletionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"generateCompletion"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"GenerateCompletionInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"generateCompletion"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"completionResponse"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"chunks"},"directives":[{"kind":"Directive","name":{"kind":"Name","value":"stream"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"content"}}]}}]}}]}}]}}]} as unknown as DocumentNode<GenerateCompletionMutation, GenerateCompletionMutationVariables>;
export const CharacterListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"characterList"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"allCharacters"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"age"}}]}}]}}]} as unknown as DocumentNode<CharacterListQuery, CharacterListQueryVariables>;
export const DetailedCharacterListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"detailedCharacterList"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"allCharacters"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"age"}},{"kind":"Field","name":{"kind":"Name","value":"details"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"key"}},{"kind":"Field","name":{"kind":"Name","value":"value"}}]}}]}}]}}]} as unknown as DocumentNode<DetailedCharacterListQuery, DetailedCharacterListQueryVariables>;
export const GetCharacterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"getCharacter"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"characterId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"character"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"characterId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"characterId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"age"}},{"kind":"Field","name":{"kind":"Name","value":"details"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"key"}},{"kind":"Field","name":{"kind":"Name","value":"value"}}]}}]}}]}}]} as unknown as DocumentNode<GetCharacterQuery, GetCharacterQueryVariables>;
export const UpdateRecipeDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateRecipe"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateRecipeInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateRecipe"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"idResponseOfRecipe"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<UpdateRecipeMutation, UpdateRecipeMutationVariables>;
export const GetMarketsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetMarkets"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"allMarkets"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"icon"}}]}}]}}]}}]} as unknown as DocumentNode<GetMarketsQuery, GetMarketsQueryVariables>;
export const GetMarketTradesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetMarketTrades"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"GetMarketTradesInput"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"where"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"GetMarketTradesResponseFilterInput"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"order"}},"type":{"kind":"ListType","type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"GetMarketTradesResponseSortInput"}}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"first"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"after"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"last"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"before"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"marketTrades"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}},{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"Variable","name":{"kind":"Name","value":"where"}}},{"kind":"Argument","name":{"kind":"Name","value":"order"},"value":{"kind":"Variable","name":{"kind":"Name","value":"order"}}},{"kind":"Argument","name":{"kind":"Name","value":"first"},"value":{"kind":"Variable","name":{"kind":"Name","value":"first"}}},{"kind":"Argument","name":{"kind":"Name","value":"after"},"value":{"kind":"Variable","name":{"kind":"Name","value":"after"}}},{"kind":"Argument","name":{"kind":"Name","value":"last"},"value":{"kind":"Variable","name":{"kind":"Name","value":"last"}}},{"kind":"Argument","name":{"kind":"Name","value":"before"},"value":{"kind":"Variable","name":{"kind":"Name","value":"before"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"totalCount"}},{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"averagePrice"}},{"kind":"Field","name":{"kind":"Name","value":"limit"}},{"kind":"Field","name":{"kind":"Name","value":"margin"}},{"kind":"Field","name":{"kind":"Name","value":"maxPrice"}},{"kind":"Field","name":{"kind":"Name","value":"minPrice"}},{"kind":"Field","name":{"kind":"Name","value":"numTransactions"}},{"kind":"Field","name":{"kind":"Name","value":"roi"}},{"kind":"Field","name":{"kind":"Name","value":"symbolCode"}},{"kind":"Field","name":{"kind":"Name","value":"symbolId"}},{"kind":"Field","name":{"kind":"Name","value":"symbolName"}},{"kind":"Field","name":{"kind":"Name","value":"symbolSubcode"}},{"kind":"Field","name":{"kind":"Name","value":"totalGain"}},{"kind":"Field","name":{"kind":"Name","value":"totalSpent"}},{"kind":"Field","name":{"kind":"Name","value":"totalVolume"}}]}},{"kind":"Field","name":{"kind":"Name","value":"pageInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"endCursor"}},{"kind":"Field","name":{"kind":"Name","value":"hasNextPage"}},{"kind":"Field","name":{"kind":"Name","value":"hasPreviousPage"}},{"kind":"Field","name":{"kind":"Name","value":"startCursor"}}]}}]}}]}}]} as unknown as DocumentNode<GetMarketTradesQuery, GetMarketTradesQueryVariables>;
export const GetRecentMarketTradesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetRecentMarketTrades"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"marketId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"first"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"allTrades"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"first"},"value":{"kind":"Variable","name":{"kind":"Name","value":"first"}}},{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"metadata"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"marketId"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"eq"},"value":{"kind":"Variable","name":{"kind":"Name","value":"marketId"}}}]}}]}}]}},{"kind":"Argument","name":{"kind":"Name","value":"order"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"createdAt"},"value":{"kind":"EnumValue","value":"DESC"}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"metadata"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"symbolId"}},{"kind":"Field","name":{"kind":"Name","value":"symbolName"}},{"kind":"Field","name":{"kind":"Name","value":"symbolSubcode"}}]}},{"kind":"Field","name":{"kind":"Name","value":"price"}},{"kind":"Field","name":{"kind":"Name","value":"volume"}}]}}]}}]}}]} as unknown as DocumentNode<GetRecentMarketTradesQuery, GetRecentMarketTradesQueryVariables>;
export const GetSymbolDetailsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetSymbolDetails"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"symbolId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"seconds"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Float"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"symbol"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"symbolId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"symbolId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"code"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"marketId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"subcode"}},{"kind":"Field","name":{"kind":"Name","value":"updatedAt"}}]}},{"kind":"Field","name":{"kind":"Name","value":"symbolTrades"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"symbolId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"symbolId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"seconds"},"value":{"kind":"Variable","name":{"kind":"Name","value":"seconds"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"totalSpent"}},{"kind":"Field","name":{"kind":"Name","value":"averagePrice"}},{"kind":"Field","name":{"kind":"Name","value":"minPrice"}},{"kind":"Field","name":{"kind":"Name","value":"maxPrice"}},{"kind":"Field","name":{"kind":"Name","value":"volume"}},{"kind":"Field","name":{"kind":"Name","value":"numTransactions"}},{"kind":"Field","name":{"kind":"Name","value":"trades"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"date"}},{"kind":"Field","name":{"kind":"Name","value":"maxPrice"}},{"kind":"Field","name":{"kind":"Name","value":"minPrice"}},{"kind":"Field","name":{"kind":"Name","value":"numTransactions"}},{"kind":"Field","name":{"kind":"Name","value":"price"}},{"kind":"Field","name":{"kind":"Name","value":"totalSpent"}},{"kind":"Field","name":{"kind":"Name","value":"volume"}}]}}]}},{"kind":"Field","alias":{"kind":"Name","value":"latestTrade"},"name":{"kind":"Name","value":"allTrades"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"first"},"value":{"kind":"IntValue","value":"1"}},{"kind":"Argument","name":{"kind":"Name","value":"order"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"createdAt"},"value":{"kind":"EnumValue","value":"DESC"}}]}},{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"metadata"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"symbolId"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"eq"},"value":{"kind":"Variable","name":{"kind":"Name","value":"symbolId"}}}]}}]}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"price"}},{"kind":"Field","name":{"kind":"Name","value":"volume"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"allForecasts"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"symbolId"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"eq"},"value":{"kind":"Variable","name":{"kind":"Name","value":"symbolId"}}}]}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"predictions"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"averagePrice"}}]}},{"kind":"Field","name":{"kind":"Name","value":"latest"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"averagePrice"}}]}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"dailySymbolSummary"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"symbolId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"symbolId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"averagePrice"}},{"kind":"Field","name":{"kind":"Name","value":"maxPrice"}},{"kind":"Field","name":{"kind":"Name","value":"minPrice"}},{"kind":"Field","name":{"kind":"Name","value":"volume"}}]}}]}}]} as unknown as DocumentNode<GetSymbolDetailsQuery, GetSymbolDetailsQueryVariables>;
export const GetRecipeTradesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetRecipeTrades"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"GetRecipeTradesInput"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"where"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"GetRecipeTradesResponseFilterInput"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"order"}},"type":{"kind":"ListType","type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"GetRecipeTradesResponseSortInput"}}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"first"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"after"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"last"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"before"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recipeTrades"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}},{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"Variable","name":{"kind":"Name","value":"where"}}},{"kind":"Argument","name":{"kind":"Name","value":"order"},"value":{"kind":"Variable","name":{"kind":"Name","value":"order"}}},{"kind":"Argument","name":{"kind":"Name","value":"first"},"value":{"kind":"Variable","name":{"kind":"Name","value":"first"}}},{"kind":"Argument","name":{"kind":"Name","value":"after"},"value":{"kind":"Variable","name":{"kind":"Name","value":"after"}}},{"kind":"Argument","name":{"kind":"Name","value":"last"},"value":{"kind":"Variable","name":{"kind":"Name","value":"last"}}},{"kind":"Argument","name":{"kind":"Name","value":"before"},"value":{"kind":"Variable","name":{"kind":"Name","value":"before"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"totalCount"}},{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"averageBuyPrice"}},{"kind":"Field","name":{"kind":"Name","value":"averageMargin"}},{"kind":"Field","name":{"kind":"Name","value":"averageSellPrice"}},{"kind":"Field","name":{"kind":"Name","value":"latestBuyPrice"}},{"kind":"Field","name":{"kind":"Name","value":"latestMargin"}},{"kind":"Field","name":{"kind":"Name","value":"latestSellPrice"}},{"kind":"Field","name":{"kind":"Name","value":"recipeId"}},{"kind":"Field","name":{"kind":"Name","value":"recipeName"}}]}},{"kind":"Field","name":{"kind":"Name","value":"pageInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"endCursor"}},{"kind":"Field","name":{"kind":"Name","value":"hasNextPage"}},{"kind":"Field","name":{"kind":"Name","value":"hasPreviousPage"}},{"kind":"Field","name":{"kind":"Name","value":"startCursor"}}]}}]}}]}}]} as unknown as DocumentNode<GetRecipeTradesQuery, GetRecipeTradesQueryVariables>;
export const GetMarketForecastDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetMarketForecast"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"marketId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"where"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"GetMarketForecastResponseFilterInput"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"order"}},"type":{"kind":"ListType","type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"GetMarketForecastResponseSortInput"}}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"first"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"after"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"last"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"before"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"marketForecast"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"marketId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"marketId"}}}]}},{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"Variable","name":{"kind":"Name","value":"where"}}},{"kind":"Argument","name":{"kind":"Name","value":"order"},"value":{"kind":"Variable","name":{"kind":"Name","value":"order"}}},{"kind":"Argument","name":{"kind":"Name","value":"first"},"value":{"kind":"Variable","name":{"kind":"Name","value":"first"}}},{"kind":"Argument","name":{"kind":"Name","value":"after"},"value":{"kind":"Variable","name":{"kind":"Name","value":"after"}}},{"kind":"Argument","name":{"kind":"Name","value":"last"},"value":{"kind":"Variable","name":{"kind":"Name","value":"last"}}},{"kind":"Argument","name":{"kind":"Name","value":"before"},"value":{"kind":"Variable","name":{"kind":"Name","value":"before"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"totalCount"}},{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"symbolId"}},{"kind":"Field","name":{"kind":"Name","value":"symbolName"}},{"kind":"Field","name":{"kind":"Name","value":"symbolSubcode"}},{"kind":"Field","name":{"kind":"Name","value":"latest"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"averagePrice"}}]}},{"kind":"Field","name":{"kind":"Name","value":"dayOne"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"averagePrice"}},{"kind":"Field","name":{"kind":"Name","value":"margin"}},{"kind":"Field","name":{"kind":"Name","value":"gain"}}]}},{"kind":"Field","name":{"kind":"Name","value":"dayTwo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"averagePrice"}},{"kind":"Field","name":{"kind":"Name","value":"margin"}},{"kind":"Field","name":{"kind":"Name","value":"gain"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"pageInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"endCursor"}},{"kind":"Field","name":{"kind":"Name","value":"hasNextPage"}},{"kind":"Field","name":{"kind":"Name","value":"hasPreviousPage"}},{"kind":"Field","name":{"kind":"Name","value":"startCursor"}}]}}]}}]}}]} as unknown as DocumentNode<GetMarketForecastQuery, GetMarketForecastQueryVariables>;
export const GetRecipeDetailsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetRecipeDetails"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"recipeId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recipe"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"recipeId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"recipeId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"cost"}},{"kind":"Field","name":{"kind":"Name","value":"inputs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"quantity"}},{"kind":"Field","name":{"kind":"Name","value":"symbolId"}}]}},{"kind":"Field","name":{"kind":"Name","value":"outputs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"quantity"}},{"kind":"Field","name":{"kind":"Name","value":"symbolId"}}]}}]}}]}}]} as unknown as DocumentNode<GetRecipeDetailsQuery, GetRecipeDetailsQueryVariables>;
export const SearchSymbolsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchSymbols"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"query"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"allSymbols"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"name"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"contains"},"value":{"kind":"Variable","name":{"kind":"Name","value":"query"}}}]}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"code"}},{"kind":"Field","name":{"kind":"Name","value":"subcode"}}]}}]}}]}}]} as unknown as DocumentNode<SearchSymbolsQuery, SearchSymbolsQueryVariables>;