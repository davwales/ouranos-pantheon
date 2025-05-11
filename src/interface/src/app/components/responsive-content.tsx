"use client";

import { useIsMobile } from "@/hooks/use-mobile";
import React from "react";

type ContentType = "mobile" | "desktop";

interface ContentProps extends React.ComponentProps<"div"> {
    type: ContentType;
}

export function Content({ type, children, ...props }: ContentProps) {
    return <div {...props}>{children}</div>;
}

export function ResponsiveContent({
    children,
    ...props
}: React.ComponentProps<"div"> & {
    children: React.ReactElement<ContentProps>[];
}) {
    const isMobile = useIsMobile();
    const targetType: ContentType = isMobile ? "mobile" : "desktop";

    const content = children.filter((child) => child.props.type === targetType);

    return (
        <div {...props}>
            {content}
        </div>
    );
}
