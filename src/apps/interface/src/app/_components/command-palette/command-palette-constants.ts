import {
  BookOpen,
  ChefHat,
  FolderKanban,
  LineChart,
  MessagesSquare,
  Navigation,
  Store,
  Tags,
  Zap,
} from "lucide-react";

export const MIN_SEARCH_LENGTH = 2;
export const SEARCH_RESULTS_PER_SOURCE = 5;

export const PALETTE_ICONS = {
  actions: Zap,
  pages: Navigation,
  markets: Store,
  symbols: LineChart,
  symbolGroups: Tags,
  strategies: FolderKanban,
  marketRecipes: BookOpen,
  kitchenRecipes: ChefHat,
  conversations: MessagesSquare,
};
