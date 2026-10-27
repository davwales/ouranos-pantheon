export type MenuAction = {
    label: string;
    icon: React.ReactNode;
    onClick: () => void;
};

export type ContextMenuProps = {
    actions: MenuAction[];
    title: string;
    description: string;
    children: React.ReactNode;
    onOpenChange?: (open: boolean) => void;
    disabled?: boolean;
};
