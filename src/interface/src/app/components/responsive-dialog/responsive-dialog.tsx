import { Content, ResponsiveContent } from '@/app/components/responsive-content';
import { DesktopDialog } from '@/app/components/responsive-dialog/desktop-dialog';
import { MobileDialog } from '@/app/components/responsive-dialog/mobile-dialog';
import { DialogProps } from '@/app/components/responsive-dialog/types';

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