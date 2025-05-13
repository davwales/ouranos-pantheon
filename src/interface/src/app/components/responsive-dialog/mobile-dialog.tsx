import { DialogProps } from '@/app/components/responsive-dialog/types';
import { Drawer, DrawerContent, DrawerDescription, DrawerHeader, DrawerTitle, DrawerTrigger } from '@/components/ui/drawer';

export function MobileDialog({
    title,
    description,
    trigger,
    children,
    open,
    onOpenChange,
    ...props
}: React.ComponentProps<typeof Drawer> & DialogProps) {
    return (
        <Drawer open={open} onOpenChange={onOpenChange} {...props}>
            <DrawerTrigger asChild>
                {trigger}
            </DrawerTrigger>
            <DrawerContent className="p-4">
                <DrawerHeader>
                    <DrawerTitle>{title}</DrawerTitle>
                    <DrawerDescription>{description}</DrawerDescription>
                </DrawerHeader>
                <div className="p-4">
                    {children}
                </div>
            </DrawerContent>
        </Drawer>
    );
}
