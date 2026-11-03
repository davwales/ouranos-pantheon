"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { type ComponentProps } from "react";
import { ORIGIN_PARAM } from "./breadcrumb-trail";

type OriginLinkProps = Omit<ComponentProps<typeof Link>, "href"> & { href: string };

export function OriginLink({ href, ...props }: OriginLinkProps) {
  const pathname = usePathname();
  const params = new URLSearchParams({ [ORIGIN_PARAM]: pathname });
  return <Link href={`${href}?${params}`} {...props} />;
}
