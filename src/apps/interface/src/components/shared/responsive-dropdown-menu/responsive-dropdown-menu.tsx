import { Content, ResponsiveContent } from "@/components/shared/responsive-content";
import { DesktopDropdownMenu } from "@/components/shared/responsive-dropdown-menu/desktop-dropdown-menu";
import { MobileDropdownMenu } from "@/components/shared/responsive-dropdown-menu/mobile-dropdown-menu";
import { DropdownMenuProps } from "@/components/shared/responsive-dropdown-menu/types";

export function ResponsiveDropdownMenu({
    title,
    description,
    actions,
    children,
    ...props
}: React.ComponentProps<"div"> & DropdownMenuProps) {
    const dropdownMenuProps: DropdownMenuProps = {
        title,
        description,
        actions,
        children,
    };

    return (
        <ResponsiveContent {...props}>
            <Content type="mobile">
                <MobileDropdownMenu {...dropdownMenuProps} />
            </Content>

            <Content type="desktop">
                <DesktopDropdownMenu {...dropdownMenuProps} />
            </Content>
        </ResponsiveContent>
    );
}
