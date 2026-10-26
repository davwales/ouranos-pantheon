import { MARKET_FEATURES } from "@/app/(plutus)/plutus/constants/market-features";
import { NAVIGATION_ITEMS } from "@/components/shared/responsive-navigation-bar";
import { THEME_OPTIONS } from "@/components/shared/theme-toggle";
import { type Market } from "@/lib/api/plutus";
import { PALETTE_ICONS } from "./command-palette-constants";
import { type PaletteEntry, type PaletteGroup } from "./command-palette-types";

const GLOBAL_CREATE_ACTIONS: { label: string; hint: string; href: string }[] = [
  { label: "New chat", hint: "Hermes", href: "/hermes/chat" },
  { label: "New kitchen recipe", hint: "Hestia", href: "/hestia/recipes/create" },
  { label: "New persona", hint: "Hermes", href: "/hermes/personas/create" },
  { label: "New model", hint: "Hermes", href: "/hermes/models/create" },
  { label: "New trait", hint: "Hermes", href: "/hermes/traits/create" },
  { label: "New LoRA", hint: "Hermes", href: "/hermes/loras/create" },
  { label: "New tool set", hint: "Hermes", href: "/hermes/tool-sets/create" },
];

const MARKET_CREATE_ACTIONS: { label: string; segment: string }[] = [
  { label: "New strategy", segment: "strategies/create" },
  { label: "New symbol group", segment: "groups/create" },
  { label: "New market recipe", segment: "recipes/create" },
];

function buildActionEntries(markets: Market[]): PaletteEntry[] {
  const globalActions = GLOBAL_CREATE_ACTIONS.map(
    ({ label, hint, href }): PaletteEntry => ({
      id: `action:${href}`,
      label,
      hint,
      target: { kind: "link", href },
    }),
  );

  const marketActions = markets.flatMap((market) =>
    MARKET_CREATE_ACTIONS.map(
      ({ label, segment }): PaletteEntry => ({
        id: `action:${market.id}:${segment}`,
        label,
        hint: market.name,
        target: { kind: "link", href: `/plutus/${market.id}/${segment}` },
      }),
    ),
  );

  const themeActions = THEME_OPTIONS.map(
    ({ value, label }): PaletteEntry => ({
      id: `theme:${value}`,
      label: `Use ${label.toLowerCase()} theme`,
      hint: "Appearance",
      keywords: ["theme", "dark mode", "appearance"],
      target: { kind: "theme", theme: value },
    }),
  );

  return [...globalActions, ...marketActions, ...themeActions];
}

function buildPageEntries(markets: Market[]): PaletteEntry[] {
  const modulePages = NAVIGATION_ITEMS.flatMap((item) =>
    item.options.map(
      (option): PaletteEntry => ({
        id: `page:${option.href}`,
        label: option.label,
        hint: item.label === option.label ? undefined : item.label,
        keywords: option.description ? [option.description] : undefined,
        target: { kind: "link", href: option.href },
      }),
    ),
  );

  const marketPages = markets.flatMap((market) =>
    MARKET_FEATURES.map(
      (feature): PaletteEntry => ({
        id: `page:${market.id}:${feature.segment}`,
        label: feature.name,
        hint: market.name,
        keywords: [feature.description],
        target: { kind: "link", href: `/plutus/${market.id}/${feature.segment}` },
      }),
    ),
  );

  return [...modulePages, ...marketPages];
}

function buildMarketEntries(markets: Market[]): PaletteEntry[] {
  return markets.map((market) => ({
    id: `market:${market.id}`,
    label: market.name,
    hint: "Market",
    keywords: market.description ? [market.description] : undefined,
    target: { kind: "link", href: `/plutus/${market.id}` },
  }));
}

export function buildStaticGroups(markets: Market[]): PaletteGroup[] {
  return [
    {
      heading: "Actions",
      icon: PALETTE_ICONS.actions,
      entries: buildActionEntries(markets),
    },
    {
      heading: "Pages",
      icon: PALETTE_ICONS.pages,
      entries: buildPageEntries(markets),
    },
    {
      heading: "Markets",
      icon: PALETTE_ICONS.markets,
      entries: buildMarketEntries(markets),
    },
  ];
}
