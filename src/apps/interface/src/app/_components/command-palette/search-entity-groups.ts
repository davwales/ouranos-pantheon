import { hermesApi } from "@/lib/api/hermes";
import { hestiaApi } from "@/lib/api/hestia";
import { type Market, plutusApi } from "@/lib/api/plutus";
import {
  PALETTE_ICONS,
  SEARCH_RESULTS_PER_SOURCE,
} from "./command-palette-constants";
import { type PaletteEntry, type PaletteGroup } from "./command-palette-types";

async function orEmpty(
  search: () => Promise<PaletteEntry[]>,
): Promise<PaletteEntry[]> {
  try {
    return await search();
  } catch {
    return [];
  }
}

function nameFilter(field: string, query: string): string[] {
  return [`${field}:like:${query}`];
}

export async function searchEntityGroups(
  query: string,
  markets: Market[],
): Promise<PaletteGroup[]> {
  const take = SEARCH_RESULTS_PER_SOURCE;
  const marketNames = new Map(markets.map((m) => [m.id, m.name]));

  const symbols = orEmpty(async () => {
    const page = await plutusApi.getAllSymbols({
      filter: nameFilter("name", query),
      take,
    });
    return page.items.map((symbol): PaletteEntry => ({
      id: `symbol:${symbol.id}`,
      label: symbol.subcode ? `${symbol.name} (${symbol.subcode})` : symbol.name,
      hint: marketNames.get(symbol.marketId),
      target: {
        kind: "link",
        href: `/plutus/${symbol.marketId}/${symbol.id}`,
      },
    }));
  });

  const symbolGroups = orEmpty(async () => {
    const pages = await Promise.all(
      markets.map((market) =>
        plutusApi.getAllSymbolGroups(market.id, "OneDay", {
          filter: nameFilter("name", query),
          take,
        }),
      ),
    );
    return pages.flatMap((page) =>
      page.items.map((group): PaletteEntry => ({
        id: `group:${group.id}`,
        label: group.name,
        hint: marketNames.get(group.marketId),
        target: {
          kind: "link",
          href: `/plutus/${group.marketId}/groups/${group.id}`,
        },
      })),
    );
  });

  const strategies = orEmpty(async () => {
    const pages = await Promise.all(
      markets.map((market) =>
        plutusApi.getAllStrategies(market.id, {
          filter: nameFilter("name", query),
          take,
        }),
      ),
    );
    return pages.flatMap((page) =>
      page.items.map((strategy): PaletteEntry => ({
        id: `strategy:${strategy.id}`,
        label: strategy.name,
        hint: marketNames.get(strategy.marketId),
        target: {
          kind: "link",
          href: `/plutus/${strategy.marketId}/strategies/${strategy.id}`,
        },
      })),
    );
  });

  const marketRecipes = orEmpty(async () => {
    const page = await plutusApi.getAllRecipes({
      filter: nameFilter("name", query),
      take,
    });
    return page.items.map((recipe): PaletteEntry => ({
      id: `market-recipe:${recipe.id}`,
      label: recipe.name,
      hint: marketNames.get(recipe.marketId),
      target: {
        kind: "link",
        href: `/plutus/${recipe.marketId}/recipes/${recipe.id}`,
      },
    }));
  });

  const kitchenRecipes = orEmpty(async () => {
    const page = await hestiaApi.getAllRecipes({
      filter: nameFilter("title", query),
      take,
    });
    return page.items.map((recipe): PaletteEntry => ({
      id: `kitchen-recipe:${recipe.id}`,
      label: recipe.title,
      hint: "Hestia",
      target: { kind: "link", href: `/hestia/recipes/${recipe.id}` },
    }));
  });

  const conversations = orEmpty(async () => {
    const items = await hermesApi.getAllConversations({
      filter: nameFilter("name", query),
      sortField: "UpdatedAt",
      sortDirection: "desc",
    });
    return items.slice(0, take).map((conversation): PaletteEntry => ({
      id: `conversation:${conversation.id}`,
      label: conversation.name,
      hint: "Hermes",
      target: {
        kind: "link",
        href: `/hermes/conversations/${conversation.id}`,
      },
    }));
  });

  const [
    symbolEntries,
    groupEntries,
    strategyEntries,
    marketRecipeEntries,
    kitchenRecipeEntries,
    conversationEntries,
  ] = await Promise.all([
    symbols,
    symbolGroups,
    strategies,
    marketRecipes,
    kitchenRecipes,
    conversations,
  ]);

  return [
    { heading: "Symbols", icon: PALETTE_ICONS.symbols, entries: symbolEntries },
    {
      heading: "Symbol Groups",
      icon: PALETTE_ICONS.symbolGroups,
      entries: groupEntries,
    },
    {
      heading: "Strategies",
      icon: PALETTE_ICONS.strategies,
      entries: strategyEntries,
    },
    {
      heading: "Market Recipes",
      icon: PALETTE_ICONS.marketRecipes,
      entries: marketRecipeEntries,
    },
    {
      heading: "Kitchen Recipes",
      icon: PALETTE_ICONS.kitchenRecipes,
      entries: kitchenRecipeEntries,
    },
    {
      heading: "Conversations",
      icon: PALETTE_ICONS.conversations,
      entries: conversationEntries,
    },
  ].filter((group) => group.entries.length > 0);
}
