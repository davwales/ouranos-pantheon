export interface MenuAction {
    label: string;
    icon: React.ReactNode;
    onClick: () => void;
}

export interface ContextMenuProps {
    actions: MenuAction[];
    title: string;
    description: string;
    children: React.ReactNode;
    onOpenChange?: (open: boolean) => void;
    disabled?: boolean;
}
