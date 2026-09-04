"use client";

import { Button } from "@/components/ui/button";
import { ShoppingListSkeleton } from "@/app/(hestia)/hestia/shopping-list/_components/shopping-list-skeleton";
import { ShoppingListView } from "@/app/(hestia)/hestia/shopping-list/_components/shopping-list-view";
import { useApi } from "@/hooks/use-api";
import { hestiaApi } from "@/lib/api/hestia";

export default function ShoppingListPage() {
  const [state, reexecute] = useApi(() => hestiaApi.getShoppingList());

  if (state.status === "loading" && !state.data) {
    return <ShoppingListSkeleton />;
  }

  if (state.status === "error") {
    return (
      <div className="m-4 space-y-4">
        <div
          role="alert"
          className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive"
        >
          Failed to load shopping list: {state.error?.message}
        </div>
        <Button onClick={reexecute}>Retry</Button>
      </div>
    );
  }

  if (!state.data) {
    return <ShoppingListSkeleton />;
  }

  return (
    <div className="m-4 space-y-4">
      <ShoppingListView data={state.data} onReload={reexecute} />
    </div>
  );
}
