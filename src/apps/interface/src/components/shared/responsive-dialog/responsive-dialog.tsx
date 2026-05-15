import { Content, ResponsiveContent } from '@/components/shared/responsive-content';
import { DesktopDialog } from '@/components/shared/responsive-dialog/desktop-dialog';
import { MobileDialog } from '@/components/shared/responsive-dialog/mobile-dialog';
import { DialogProps } from '@/components/shared/responsive-dialog/types';

export function ResponsiveDialog({
    title,
    description,
    trigger,
    children,
    open,
    onOpenChange,
    ...props
}: React.ComponentProps<"div"> & DialogProps) {
    const dialogProps: DialogProps = {
        title,
        description,
        trigger,
        children,
        open,
        onOpenChange,
    };

    return (
        <ResponsiveContent {...props}>
            <Content type="mobile">
                <MobileDialog {...dialogProps} />
            </Content>

            <Content type="desktop">
                <DesktopDialog {...dialogProps} />
            </Content>
        </ResponsiveContent>
    );
};