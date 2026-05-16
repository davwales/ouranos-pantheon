import { Content, ResponsiveContent } from '@/components/shared/responsive-content';
import { DesktopContextMenu } from '@/components/shared/responsive-context-menu/desktop-context-menu';
import { MobileContextMenu } from '@/components/shared/responsive-context-menu/mobile-context-menu';
import { ContextMenuProps } from '@/components/shared/responsive-context-menu/types';

export function ResponsiveContextMenu({
    actions,
    title,
    description,
    children,
    onOpenChange,
    disabled,
    ...props
}: React.ComponentProps<"div"> & ContextMenuProps) {
    const contextMenuProps: ContextMenuProps = {
        actions,
        title,
        description,
        onOpenChange,
        disabled,
        children
    };

    return (
        <ResponsiveContent {...props}>
            <Content type="mobile">
                <MobileContextMenu {...contextMenuProps} />
            </Content>

            <Content type="desktop">
                <DesktopContextMenu {...contextMenuProps} />
            </Content>
        </ResponsiveContent>
    );
};