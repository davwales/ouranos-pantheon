export interface RecipeSymbol {
    name: string;
    quantity: number;
    symbolId: string;
}

export interface Recipe {
    id: string;
    name: string;
    cost: number;
    inputs: RecipeSymbol[];
    outputs: RecipeSymbol[];
}