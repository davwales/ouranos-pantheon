import { NavigationBarItem } from "@/app/components/responsive-navigation-bar/types";
import { Typography } from "@/app/components/typography";
import {
  NavigationMenu,
  NavigationMenuContent,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
  NavigationMenuTrigger,
  navigationMenuTriggerStyle,
} from "@/components/ui/navigation-menu";
import { cn } from "@/lib/utils";
import Link from "next/link";
import React from "react";

export default function DesktopNavigationBar({
  items,
  className,
}: React.ComponentProps<"nav"> & {
  items: NavigationBarItem[];
}) {
  return (
    <NavigationMenu className={className}>
      <NavigationMenuList>
        {items.map((item, itemIndex) =>
          item.options.length > 1 ? (
            <NavigationMenuItem key={itemIndex}>
              <NavigationMenuTrigger>{item.label}</NavigationMenuTrigger>
              <NavigationMenuContent>
                <ul className="grid w-[400px] gap-3 p-4 md:w-[500px] md:grid-cols-2 lg:w-[600px]">
                  {item.options.map((option, optionIndex) => (
                    <ListItem
                      key={optionIndex}
                      title={option.label}
                      href={option.href}
                    >
                      {option.description}
                    </ListItem>
                  ))}
                </ul>
              </NavigationMenuContent>
            </NavigationMenuItem>
          ) : (
            <NavigationMenuItem key={itemIndex}>
              <NavigationMenuLink
                asChild
                className={navigationMenuTriggerStyle()}
              >
                <Link href={item.options[0].href}>{item.label}</Link>
              </NavigationMenuLink>
            </NavigationMenuItem>
          ),
        )}
      </NavigationMenuList>
    </NavigationMenu>
  );
}

function ListItem({
  className,
  title,
  children,
  ...props
}: React.ComponentProps<"a">) {
  return (
    <li>
      <NavigationMenuLink asChild>
        <a
          className={cn(
            "block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground",
            className,
          )}
          {...props}
        >
          <Typography variant="small">{title}</Typography>
          <Typography variant="muted" className="line-clamp-2 leading-snug">
            {children}
          </Typography>
        </a>
      </NavigationMenuLink>
    </li>
  );
}
