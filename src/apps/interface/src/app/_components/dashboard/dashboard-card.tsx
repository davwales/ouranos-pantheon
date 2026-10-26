import { TextSkeleton } from "@/components/shared/skeletons/text-skeleton";
import { Typography } from "@/components/shared/typography";
import {
  Card,
  CardAction,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { type ApiState } from "@/hooks/use-api";
import { type LucideIcon } from "lucide-react";
import Link from "next/link";
import { type ReactNode } from "react";

export type DashboardCardProps<T> = {
  title: string;
  icon: LucideIcon;
  href?: string;
  state: ApiState<T>;
  isEmpty: (data: T) => boolean;
  emptyText: string;
  children: (data: T) => ReactNode;
};

export function DashboardCard<T>({
  title,
  icon: Icon,
  href,
  state,
  isEmpty,
  emptyText,
  children,
}: DashboardCardProps<T>) {
  return (
    <Card className="gap-4">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Icon className="size-4 text-muted-foreground" />
          {title}
        </CardTitle>
        {href && (
          <CardAction>
            <Link
              href={href}
              className="text-sm text-muted-foreground hover:text-foreground"
            >
              View all
            </Link>
          </CardAction>
        )}
      </CardHeader>
      <CardContent>
        <DashboardCardBody state={state} isEmpty={isEmpty} emptyText={emptyText}>
          {children}
        </DashboardCardBody>
      </CardContent>
    </Card>
  );
}

function DashboardCardBody<T>({
  state,
  isEmpty,
  emptyText,
  children,
}: Pick<DashboardCardProps<T>, "state" | "isEmpty" | "emptyText" | "children">) {
  if (state.data === undefined) {
    return state.status === "error" ? (
      <Typography variant="muted" className="text-destructive">
        Couldn’t load this right now.
      </Typography>
    ) : (
      <TextSkeleton lines={3} />
    );
  }

  if (isEmpty(state.data)) {
    return <Typography variant="muted">{emptyText}</Typography>;
  }

  return children(state.data);
}
