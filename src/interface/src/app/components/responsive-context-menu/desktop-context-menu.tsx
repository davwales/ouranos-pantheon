import { ContextMenuProps } from "@/app/components/responsive-context-menu/types";
import { ContextMenu, ContextMenuTrigger } from "@/components/ui/context-menu";
import { ContextMenuContent, ContextMenuItem } from "@radix-ui/react-context-menu";

export function DesktopContextMenu({
    actions,
    children,
    disabled,
    ...props
}: React.ComponentProps<typeof ContextMenu> & ContextMenuProps) {
    return (
        <ContextMenu {...props}>
            <ContextMenuTrigger disabled={disabled}>
                {children}
            </ContextMenuTrigger>
            <ContextMenuContent>
                <div className="m-2 border bg-background rounded">
                    {actions.map((action, index) => (
                        <ContextMenuItem
                            key={index}
                            onClick={action.onClick}
                            className="flex items-center gap-2 m-2 p-2 rounded hover:bg-accent hover:cursor-pointer"
                        >
                            {action.icon}
                            {action.label}
                        </ContextMenuItem>
                    ))}
                </div>
            </ContextMenuContent>
        </ContextMenu>
    );
}