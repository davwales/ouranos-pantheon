"use client";

import { Typography } from "@/components/shared/typography";
import { SidebarMenuButton, useSidebar } from "@/components/ui/sidebar";
import Link from "next/link";
import React from "react";

export default function SidebarLink({
    label,
    href,
    className,
    ...props
}: React.ComponentProps<typeof SidebarMenuButton> & {
    label: string;
    href: string;
}) {
    const { toggleSidebar } = useSidebar();

    return (
        <SidebarMenuButton {...props} onClick={toggleSidebar} className={`py-5 ${className}`}>
            <Link href={href} passHref>
                <Typography variant="h4">{label}</Typography>
            </Link>
        </SidebarMenuButton>
    );
}