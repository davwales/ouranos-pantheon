"use client";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import { ChevronRight, MoreHorizontal } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useBreadcrumbStore } from "@/stores/breadcrumb-store";
import { buildBreadcrumbTrail, type BreadcrumbCrumb } from "./breadcrumb-trail";

const INLINE_CRUMBS_ON_MOBILE = 2;

function CrumbLabel({ crumb }: { crumb: BreadcrumbCrumb }) {
  const registered = useBreadcrumbStore((state) => state.labels[crumb.segment]);
  const label = crumb.label ?? registered;
  if (label === undefined) {
    return (
      <>
        <span
          data-slot="skeleton"
          className="inline-block h-4 w-24 animate-pulse rounded-md bg-muted-foreground/15 align-middle"
        />
        <span className="sr-only">Loading</span>
      </>
    );
  }
  return <>{label}</>;
}

function CrumbSeparator({ className }: { className?: string }) {
  return (
    <ChevronRight
      className={cn("size-3.5 shrink-0 text-muted-foreground/60", className)}
      aria-hidden="true"
    />
  );
}

function CrumbOverflowMenu({ crumbs }: { crumbs: BreadcrumbCrumb[] }) {
  return (
    <li className="flex shrink-0 items-center gap-1.5 md:hidden">
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="ghost"
            size="icon"
            className="size-8 md:size-6"
            aria-label="Show full path"
          >
            <MoreHorizontal className="size-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="start">
          {crumbs.map((crumb) => (
            <DropdownMenuItem key={crumb.href} asChild>
              <Link href={crumb.href}>
                <CrumbLabel crumb={crumb} />
              </Link>
            </DropdownMenuItem>
          ))}
        </DropdownMenuContent>
      </DropdownMenu>
      <CrumbSeparator />
    </li>
  );
}

function CrumbItem({
  crumb,
  isLast,
  hiddenOnMobile,
}: {
  crumb: BreadcrumbCrumb;
  isLast: boolean;
  hiddenOnMobile: boolean;
}) {
  return (
    <li
      className={cn(
        "flex min-w-0 items-center gap-1.5",
        hiddenOnMobile && "hidden md:flex",
      )}
    >
      {isLast ? (
        <span className="truncate font-medium" aria-current="page">
          <CrumbLabel crumb={crumb} />
        </span>
      ) : (
        <>
          <Link
            href={crumb.href}
            className="block max-w-28 truncate leading-8 text-muted-foreground transition-colors hover:text-foreground sm:max-w-48 md:leading-normal"
          >
            <CrumbLabel crumb={crumb} />
          </Link>
          <CrumbSeparator />
        </>
      )}
    </li>
  );
}

export function Breadcrumbs() {
  const crumbs = buildBreadcrumbTrail(usePathname());
  if (crumbs.length < 2) {
    return null;
  }

  const collapsedCount = Math.max(crumbs.length - INLINE_CRUMBS_ON_MOBILE, 0);

  return (
    <nav
      aria-label="Breadcrumb"
      data-slot="breadcrumbs"
      className="bg-muted/30 px-3 py-1 md:py-1.5"
    >
      <ol className="flex min-w-0 items-center gap-1.5 overflow-hidden text-sm">
        {collapsedCount > 0 && (
          <CrumbOverflowMenu crumbs={crumbs.slice(0, collapsedCount)} />
        )}
        {crumbs.map((crumb, index) => (
          <CrumbItem
            key={crumb.href}
            crumb={crumb}
            isLast={index === crumbs.length - 1}
            hiddenOnMobile={index < collapsedCount}
          />
        ))}
      </ol>
    </nav>
  );
}
