"use client";

import { Typography } from "@/components/shared/typography";
import { Button } from "@/components/ui/button";
import { Plus, RefreshCw } from "lucide-react";

type PortfolioHeaderProps = {
  fetching: boolean;
  onCreatePosition: () => void;
  onRefresh: () => void;
};

export function PortfolioHeader({
  fetching,
  onCreatePosition,
  onRefresh,
}: PortfolioHeaderProps) {
  return (
    <div className="flex items-center gap-2 justify-between">
      <Typography variant="lead">Portfolio</Typography>
      <div className="flex items-center gap-4">
        <Button
          variant="link"
          className="flex items-end gap-0"
          onClick={onCreatePosition}
        >
          <Plus className="w-4 h-4 mr-1" />
          Create Position
        </Button>
        {fetching ? (
          <RefreshCw className="animate-spin" />
        ) : (
          <RefreshCw onClick={onRefresh} className="hover:cursor-pointer" />
        )}
      </div>
    </div>
  );
}
