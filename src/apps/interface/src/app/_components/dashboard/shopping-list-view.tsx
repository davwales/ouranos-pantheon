"use client";

import { useApi } from "@/hooks/use-api";
import { hestiaApi } from "@/lib/api/hestia";
import { ShoppingCart } from "lucide-react";
import { DashboardCard } from "./dashboard-card";
import { DashboardRow } from "./dashboard-row";
import { summarizeShoppingList } from "./summarize-shopping-list";

export function ShoppingListView() {
  const [state] = useApi(() => hestiaApi.getShoppingList(), []);

  return (
    <DashboardCard
      title="Shopping List"
      icon={ShoppingCart}
      href="/hestia/shopping-list"
      state={state}
      isEmpty={(data) => summarizeShoppingList(data).totalItems === 0}
      emptyText="Your shopping list is empty."
    >
      {(data) => {
        const summary = summarizeShoppingList(data);
        return (
          <div className="space-y-3">
            <p className="text-sm">
              <span className="text-2xl font-semibold tabular-nums">
                {summary.remainingItems}
              </span>{" "}
              of {summary.totalItems} items left to buy
            </p>
            {data.recipes.length > 0 && (
              <ul>
                {data.recipes.map((recipe) => (
                  <DashboardRow
                    key={recipe.id}
                    href={`/hestia/recipes/${recipe.id}`}
                    primary={recipe.title}
                  />
                ))}
              </ul>
            )}
          </div>
        );
      }}
    </DashboardCard>
  );
}
