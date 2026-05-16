import { MenuAction } from "@/components/shared/responsive-context-menu";
import { DropdownMenuProps } from "@/components/shared/responsive-dropdown-menu/types";
import { Drawer, DrawerContent, DrawerDescription, DrawerHeader, DrawerTitle, DrawerTrigger } from "@/components/ui/drawer";
import { useState } from "react";

export function MobileDropdownMenu({
    title,
    description,
    actions,
    children,
    ...props
}: React.ComponentProps<typeof Drawer> & DropdownMenuProps) {
    const [open, setOpen] = useState<boolean>(false);

    const handleActionClicked = (item: MenuAction) => {
        item.onClick();
        setOpen(false);
    };

    return (
        <Drawer open={open} onOpenChange={setOpen} {...props}>
            <DrawerTrigger asChild>
                {children}
            </DrawerTrigger>

            <DrawerContent>
                <div className="m-4">
                    <DrawerHeader>
                        <DrawerTitle>
                            {title}
                        </DrawerTitle>
                        <DrawerDescription hidden>
                            {description}
                        </DrawerDescription>
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
