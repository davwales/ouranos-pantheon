export type RecipeSymbol = {
  name: string;
  quantity: number;
  symbolId: string;
  latestPrice?: number | null;
  averagePrice?: number | null;
  totalValue?: number | null;
  volume?: number | null;
};

export type Recipe = {
  id: string;
  name: string;
  cost: number;
  inputs: RecipeSymbol[];
  outputs: RecipeSymbol[];
};
