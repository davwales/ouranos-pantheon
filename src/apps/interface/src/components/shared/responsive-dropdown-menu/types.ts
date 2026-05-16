export type MenuAction = {
    label: string;
    icon: React.ReactNode;
    onClick: () => void;
}

export type DropdownMenuProps = {
    title: string;
    description: string;
    actions: MenuAction[];
    children: React.ReactNode;
}
