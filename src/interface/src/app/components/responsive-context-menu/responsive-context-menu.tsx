import { DesktopContent, MobileContent, ResponsiveContent } from '@/app/components/responsive-content';
import { DesktopContextMenu } from '@/app/components/responsive-context-menu/desktop-context-menu';
import { MobileContextMenu } from '@/app/components/responsive-context-menu/mobile-context-menu';
import { MenuAction } from '@/app/components/responsive-context-menu/types';

export function ResponsiveContextMenu({
    actions,
    title,
    description,
    children,
    onOpenChange,
    ...props
}: React.ComponentProps<"div"> & {
    actions: MenuAction[];
    title: string;
    description: string;
    children: React.ReactNode;
    onOpenChange?: (open: boolean) => void;
}) {
    return (
        <ResponsiveContent {...props}>
            <MobileContent>
                <MobileContextMenu
                    actions={actions}
                    title={title}
                    description={description}
                    onOpenChange={onOpenChange}
                >
                    {children}
                </MobileContextMenu>
            </MobileContent>

            <DesktopContent>
                <DesktopContextMenu
                    actions={actions}
                    onOpenChange={onOpenChange}
                >
                    {children}
                </DesktopContextMenu>
            </DesktopContent>
        </ResponsiveContent>
    );
};