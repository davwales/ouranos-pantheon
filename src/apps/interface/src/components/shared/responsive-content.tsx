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

    const content = React.Children.toArray(children).filter(
      (child) =>
        React.isValidElement(child) &&
        (child.props as Record<string, unknown>).type === targetType,
    );

    return (
        <div {...props}>
            {content}
        </div>
    );
}
