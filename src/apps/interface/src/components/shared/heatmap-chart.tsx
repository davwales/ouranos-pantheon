"use client";

import { Fragment } from "react";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";

export interface HeatmapChartCell {
  x: number;
  y: number;
  value: number;
  percentage: number;
}

export interface HeatmapChartProps {
  data: HeatmapChartCell[];
  xLabels: string[];
  yLabels: string[];
  xCount?: number;
  yCount?: number;
  levels?: number;
  colorMin?: number;
  colorMax?: number;
  highlightX?: number;
  highlightY?: number;
  className?: string;
}

function buildGrid(
  data: HeatmapChartCell[],
  xCount: number,
  yCount: number,
): HeatmapChartCell[][] {
  const grid: HeatmapChartCell[][] = [];
  for (let y = 0; y < yCount; y++) {
    const row: HeatmapChartCell[] = [];
    for (let x = 0; x < xCount; x++) {
      row.push({ x, y, value: 0, percentage: 0 });
    }
    grid.push(row);
  }

  for (const cell of data) {
    if (cell.x >= 0 && cell.x < xCount && cell.y >= 0 && cell.y < yCount) {
      grid[cell.y][cell.x] = cell;
    }
  }

  return grid;
}

function buildLevelColors(levels: number, colorMin: number, colorMax: number): string[] {
  const colors: string[] = [""];
  for (let i = 1; i <= levels; i++) {
    const pct = Math.round(colorMin + ((colorMax - colorMin) * (i - 1)) / (levels - 1));
    colors.push(
      `color-mix(in oklch, var(--chart-4) ${pct}%, var(--background))`,
    );
  }
  return colors;
}

function getColorLevel(
  percentage: number,
  maxPct: number,
  levels: number,
): number {
  if (percentage === 0) return 0;
  const ratio = percentage / maxPct;
  return Math.min(Math.ceil(ratio * levels), levels);
}

export function HeatmapChart({
  data,
  xLabels,
  yLabels,
  xCount,
  yCount,
  highlightX,
  highlightY,
  className,
  levels: rawLevels = 5,
  colorMin: rawColorMin = 20,
  colorMax: rawColorMax = 100,
}: HeatmapChartProps) {
  const cols = xCount ?? xLabels.length;
  const rows = yCount ?? yLabels.length;
  const levels = Math.max(rawLevels ?? 5, 2);
  const colorMin = Math.max(0, Math.min(rawColorMin, 100));
  const colorMax = Math.max(colorMin, Math.min(rawColorMax, 100));
  const levelColors = buildLevelColors(levels, colorMin, colorMax);
  const grid = buildGrid(data, cols, rows);
  const maxPercentage = Math.max(...grid.flat().map((c) => c.percentage), 1);

  return (
    <div className={cn("overflow-x-auto", className)}>
      <div className="min-w-80">
        <div
          className="grid gap-px"
          style={{
            gridTemplateColumns: `auto repeat(${cols}, minmax(0, 1fr))`,
            gridTemplateRows: `repeat(${rows + 1}, auto)`,
          }}
        >
          <div />
          {xLabels.map((name) => (
            <div
              key={name}
              className="py-0.5 text-center text-xs font-medium text-muted-foreground"
            >
              {name}
            </div>
          ))}
          {grid.map((row, y) => (
            <Fragment key={y}>
              <div className="self-center pr-1.5 text-right text-[10px] text-muted-foreground">
                {yLabels[y]}
              </div>
              {row.map((cell, x) => {
                const isHighlighted = x === highlightX && y === highlightY;
                const level = getColorLevel(cell.percentage, maxPercentage, levels);
                return (
                  <TooltipProvider delayDuration={0} key={x}>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <div
                          className={cn(
                            "h-3 rounded-sm transition duration-150 hover:brightness-125",
                            level === 0 && "bg-muted/30",
                            isHighlighted &&
                              "relative z-10 ring-2 ring-chart-3 ring-offset-2 ring-offset-background",
                          )}
                          style={
                            level > 0
                              ? { backgroundColor: levelColors[level] }
                              : undefined
                          }
                          role="img"
                          aria-label={`${xLabels[x]} ${yLabels[y]}, ${cell.value} trades, ${cell.percentage} percent`}
                        />
                      </TooltipTrigger>
                      <TooltipContent side="top">
                        {xLabels[x]} {yLabels[y]} - {cell.value.toLocaleString()}{" "}
                        ({cell.percentage}%)
                      </TooltipContent>
                    </Tooltip>
                  </TooltipProvider>
                );
              })}
            </Fragment>
          ))}
        </div>

        <div className="mt-2 flex items-center gap-1.5">
          <span className="text-[10px] text-muted-foreground">Less</span>
          {Array.from({ length: levels }, (_, i) => i + 1).map((level) => (
            <div
              key={level}
              className="h-1.5 w-3 rounded-sm"
              style={{ backgroundColor: levelColors[level] }}
            />
          ))}
          <span className="text-[10px] text-muted-foreground">More</span>
        </div>
      </div>
    </div>
  );
}

export function HeatmapChartSkeleton({
  xCount,
  yCount,
  levels = 5,
  className,
}: {
  xCount: number;
  yCount: number;
  levels?: number;
  className?: string;
}) {
  return (
    <div className={cn("overflow-x-auto", className)} aria-hidden="true">
      <div className="min-w-80">
        <div
          className="grid gap-px"
          style={{
            gridTemplateColumns: `auto repeat(${xCount}, minmax(0, 1fr))`,
            gridTemplateRows: `repeat(${yCount + 1}, auto)`,
          }}
        >
          <div />
          {Array.from({ length: xCount }).map((_, i) => (
            <div key={i} className="py-0.5 text-center">
              <Skeleton className="h-4 w-full" />
            </div>
          ))}
          {Array.from({ length: yCount }).map((_, y) => (
            <Fragment key={y}>
              <div className="self-center pr-1.5 text-right">
                <Skeleton className="h-4 w-8" />
              </div>
              {Array.from({ length: xCount }).map((_, x) => (
                <Skeleton key={x} className="h-3 rounded-sm" />
              ))}
            </Fragment>
          ))}
        </div>

        <div className="mt-2 flex items-center gap-1.5">
          <Skeleton className="h-1.5 w-6" />
          {Array.from({ length: levels }).map((_, i) => (
            <Skeleton key={i} className="h-1.5 w-3 rounded-sm" />
          ))}
          <Skeleton className="h-1.5 w-6" />
        </div>
      </div>
    </div>
  );
}
