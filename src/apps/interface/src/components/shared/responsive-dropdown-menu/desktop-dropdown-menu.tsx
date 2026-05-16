import { DropdownMenuProps } from "@/components/shared/responsive-dropdown-menu/types";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";

export function DesktopDropdownMenu({
    title,
    actions,
    children,
    ...props
}: React.ComponentProps<typeof DropdownMenu> & DropdownMenuProps) {
    return (
        <DropdownMenu {...props}>
            <DropdownMenuTrigger asChild>
                {children}
            </DropdownMenuTrigger>

            <DropdownMenuContent className="m-2 border bg-background rounded">
                <DropdownMenuLabel hidden>{title}</DropdownMenuLabel>

                {actions.map((action, index) => (
                    <DropdownMenuItem
                        key={index}
                        onClick={action.onClick}
                        className="flex items-center gap-2 m-2 p-2 rounded hover:bg-accent hover:cursor-pointer"
                    >
                        {action.icon}
                        {action.label}
                    </DropdownMenuItem>
                ))}
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
