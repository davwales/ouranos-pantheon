import { type NavigationBarItem } from "./types";

export const NAVIGATION_ITEMS: NavigationBarItem[] = [
  {
    label: "Home",
    options: [
      {
        label: "Home",
        description: "Dashboard across every module.",
        href: "/",
      },
    ],
  },
  {
    label: "Hermes",
    options: [
      {
        label: "Chat",
        description: "Have a conversation with a virtual assistant.",
        href: "/hermes/chat",
      },
      {
        label: "Saved Conversations",
        description: "View and resume your saved conversations.",
        href: "/hermes/conversations",
      },
      {
        label: "Manage Personas",
        description: "Configure personas you can interact with.",
        href: "/hermes/personas",
      },
      {
        label: "Manage Models",
        description: "Configure LLM models and their parameters.",
        href: "/hermes/models",
      },
      {
        label: "Manage Traits",
        description: "Create and manage conversation traits.",
        href: "/hermes/traits",
      },
      {
        label: "Manage LoRAs",
        description: "Register LoRAs that can restyle generated images.",
        href: "/hermes/loras",
      },
      {
        label: "Manage Tool Sets",
        description: "Choose which tools a model can call and how to use them.",
        href: "/hermes/tool-sets",
      },
    ],
  },
  {
    label: "Plutus",
    options: [
      {
        label: "Markets",
        description: "Select a market to explore and analyze.",
        href: "/plutus",
      },
    ],
  },
  {
    label: "Hestia",
    options: [
      {
        label: "Recipes",
        description: "Browse your saved recipes.",
        href: "/hestia/recipes",
      },
      {
        label: "Shopping List",
        description: "Build a grocery list from saved recipes.",
        href: "/hestia/shopping-list",
      },
    ],
  },
];
