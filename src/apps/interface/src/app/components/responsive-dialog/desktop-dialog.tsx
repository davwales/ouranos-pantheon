import { DialogProps } from '@/app/components/responsive-dialog/types';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';

export function DesktopDialog({
    title,
    description,
    trigger,
    children,
    open,
    onOpenChange,
    ...props
}: React.ComponentProps<typeof Dialog> & DialogProps) {
    return (
        <Dialog open={open} onOpenChange={onOpenChange} {...props}>
            <DialogTrigger asChild>
                {trigger}
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{title}</DialogTitle>
                    <DialogDescription>{description}</DialogDescription>
                </DialogHeader>
                {children}
            </DialogContent>
        </Dialog>
    );
}
