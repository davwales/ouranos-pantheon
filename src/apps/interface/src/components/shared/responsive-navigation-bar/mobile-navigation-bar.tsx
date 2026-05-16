import SidebarLink from "@/components/shared/responsive-navigation-bar/sidebar-link";
import { NavigationBarItem } from "@/components/shared/responsive-navigation-bar/types";
import { Typography } from "@/components/shared/typography";
import { Sidebar, SidebarContent, SidebarGroup, SidebarGroupContent, SidebarGroupLabel, SidebarMenu, SidebarMenuItem, SidebarTrigger } from "@/components/ui/sidebar";

export default function MobileNavigationBar({
    items,
    ...props
}: React.ComponentProps<"div"> & {
    items: NavigationBarItem[]
}) {
    return (
        <div {...props}>
            <SidebarTrigger size="lg" />
            <Sidebar>
                <SidebarContent className="mt-2">
                    {items.map((item, itemIndex) => (
                        item.options.length > 1 ? (
                            <SidebarGroup key={itemIndex}>
                                <SidebarGroupLabel>
                                    <Typography variant="h2" className="border-b-0">
                                        {item.label}
                                    </Typography>
                                </SidebarGroupLabel>
                                <SidebarGroupContent>
                                    <SidebarMenu>
                                        {item.options.map((option, optionIndex) => (
                                            <SidebarMenuItem key={optionIndex}>
                                                <SidebarLink label={option.label} href={option.href} />
                                            </SidebarMenuItem>
                                        ))}
                                    </SidebarMenu>
                                </SidebarGroupContent>
                            </SidebarGroup>
                        ) : (
                            <SidebarGroup key={itemIndex}>
                                <SidebarGroupContent>
                                    <SidebarMenu>
                                        <SidebarMenuItem>
                                            <SidebarLink label={item.label} href={item.options[0].href} />
                                        </SidebarMenuItem>
                                    </SidebarMenu>
                                </SidebarGroupContent>
                            </SidebarGroup>
                        )
                    ))}
                </SidebarContent>
            </Sidebar>
        </div>
    );
}