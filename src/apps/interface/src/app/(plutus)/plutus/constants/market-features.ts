export type MarketFeature = {
  name: string;
  description: string;
  segment: string;
};

export const MARKET_FEATURES: MarketFeature[] = [
  {
    name: "Explorer",
    description:
      "Explore market data and gain insights into pricing trends to make informed investment decisions.",
    segment: "explorer",
  },
  {
    name: "Recipes",
    description:
      "Discover how you can combine different symbols to exploit a margin.",
    segment: "recipes",
  },
  {
    name: "Forecasts",
    description: "Get a glimpse of the future with our AI-powered forecasts.",
    segment: "forecasts",
  },
  {
    name: "Recent Trades",
    description:
      "View trades as they come in, gaining insights into the market before anyone else.",
    segment: "recent",
  },
  {
    name: "Signal Rankings",
    description:
      "See which symbols have the strongest signals across all investment intents.",
    segment: "signals",
  },
  {
    name: "Groups",
    description:
      "Organize symbols into custom groups like wishlists or categories to browse related items together.",
    segment: "groups",
  },
  {
    name: "Strategies",
    description:
      "Define and manage trading strategies. Configure parameters, toggle active status, and review past performance.",
    segment: "strategies",
  },
  {
    name: "Portfolio",
    description:
      "Track and manage your positions. Create, edit, close, and link buy and sell positions.",
    segment: "portfolio",
  },
];
