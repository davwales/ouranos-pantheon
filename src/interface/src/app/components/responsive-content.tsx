import React from "react";

export function MobileContent({
    children
}: {
    children: React.ReactNode
}) {
    return children;
};

export function DesktopContent({
    children
}: {
    children: React.ReactNode
}) {
    return children;
};

export function ResponsiveContent({
    children,
    ...props
}: React.ComponentProps<"div"> & {
    children: React.ReactNode[];
}) {
    const desktopContent = children.find(
        (child) => React.isValidElement(child) && child.type === DesktopContent
    );

    const mobileContent = children.find(
        (child) => React.isValidElement(child) && child.type === MobileContent
    );

    return (
        <div {...props}>
            <div className="hidden md:block">{desktopContent}</div>
            <div className="md:hidden">{mobileContent}</div>
        </div>
    );
}
