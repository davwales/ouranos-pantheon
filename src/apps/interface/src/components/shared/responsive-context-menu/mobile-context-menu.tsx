import { ContextMenuProps, MenuAction } from "@/components/shared/responsive-context-menu/types";
import { Drawer, DrawerContent, DrawerDescription, DrawerHeader, DrawerTitle, DrawerTrigger } from "@/components/ui/drawer";
import { useState } from "react";

export function MobileContextMenu({
    actions,
    title,
    description,
    children,
    onOpenChange,
    disabled,
    ...props
}: React.ComponentProps<typeof Drawer> & ContextMenuProps) {
    const [open, setOpen] = useState<boolean>(false);

    const handleOpenChange = (updatedOpen: boolean) => {
        setOpen(updatedOpen);

        if (onOpenChange) {
            onOpenChange(updatedOpen);
        }
    };

    const handleActionClicked = (action: MenuAction) => {
        action.onClick();
        setOpen(false);
    };

    return (
        <Drawer open={open} onOpenChange={handleOpenChange} {...props}>
            <DrawerTrigger disabled={disabled} className="w-full">
                {children}
            </DrawerTrigger>
            <DrawerContent>
                <div className="m-4">
                    <DrawerHeader>
                        <DrawerTitle>{title}</DrawerTitle>
                        <DrawerDescription>{description}</DrawerDescription>
                    </DrawerHeader>
                    <div className="m-2">
                        {actions.map((action, index) => (
                            <div
                                key={index}
                                onClick={() => handleActionClicked(action)}
                                className="flex gap-4 p-2 items-center hover:cursor-pointer"
                            >
                                {action.icon}
                                {action.label}
                            </div>
                        ))}
                    </div>
                </div>
            </DrawerContent>
        </Drawer>
    );
}