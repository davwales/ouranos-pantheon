import { DesktopContent, MobileContent, ResponsiveContent } from "@/app/components/responsive-content";
import DesktopNavigationBar from "@/app/components/responsive-navigation-bar/desktop-navigation-bar";
import MobileNavigationBar from "@/app/components/responsive-navigation-bar/mobile-navigation-bar";
import { NavigationBarItem } from "@/app/components/responsive-navigation-bar/types";
import { Typography } from "@/app/components/typography";
import { Avatar, AvatarImage } from "@/components/ui/avatar";
import React from "react";

export default function ResponsiveNavigationBar({
    items,
    iconSrc,
    ...props
}: React.ComponentProps<"div"> & {
    items: NavigationBarItem[],
    iconSrc?: string
}) {
    return (
        <div className="border-b-2 p-3 flex items-center justify-between" {...props}>
            <ResponsiveContent>
                <DesktopContent>
                    <DesktopNavigationBar items={items} />
                </DesktopContent>
                <MobileContent>
                    <MobileNavigationBar items={items} />
                </MobileContent>
            </ResponsiveContent>

            <div className="flex items-center space-x-3">
                <Typography variant="h3">Ouranos</Typography>
                <Avatar>
                    <AvatarImage src={iconSrc || "/ouranos-icon.png"} />
                </Avatar>
            </div>
        </div >
    );
}