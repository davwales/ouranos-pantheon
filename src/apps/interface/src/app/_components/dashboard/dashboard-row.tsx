import Link from "next/link";
import { type ReactNode } from "react";

export type DashboardRowProps = {
  href: string;
  primary: ReactNode;
  secondary?: ReactNode;
  trailing?: ReactNode;
};

export function DashboardRow({
  href,
  primary,
  secondary,
  trailing,
}: DashboardRowProps) {
  return (
    <li>
      <Link
        href={href}
        className="-mx-2 flex items-center justify-between gap-3 rounded-md px-2 py-1.5 hover:bg-accent"
      >
        <div className="min-w-0">
          <div className="truncate text-sm font-medium">{primary}</div>
          {secondary && (
            <div className="truncate text-xs text-muted-foreground">
              {secondary}
            </div>
          )}
        </div>
        {trailing && (
          <div className="shrink-0 text-right text-sm tabular-nums">
            {trailing}
          </div>
        )}
      </Link>
    </li>
  );
}
