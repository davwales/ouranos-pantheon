"use client";

import { useNavBarActions } from "@/app/components/nav-bar-actions-context";
import {
  Content,
  ResponsiveContent,
} from "@/app/components/responsive-content";
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
  items: NavigationBarItem[];
  iconSrc?: string;
}) {
  const { actions } = useNavBarActions();

  return (
    <div
      className="border-b-2 p-3 flex items-center justify-between"
      {...props}
    >
      <ResponsiveContent>
        <Content type="desktop">
          <DesktopNavigationBar items={items} />
        </Content>
        <Content type="mobile">
          <MobileNavigationBar items={items} />
        </Content>
      </ResponsiveContent>

      <div className="flex items-center space-x-3">
        {actions}
        <Typography variant="h3">Ouranos</Typography>
        <Avatar>
          <AvatarImage src={iconSrc || "/ouranos-icon.png"} />
        </Avatar>
      </div>
    </div>
  );
}
