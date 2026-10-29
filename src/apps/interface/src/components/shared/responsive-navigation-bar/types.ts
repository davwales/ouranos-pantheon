export type NavigationBarItemOption = {
    label: string;
    description?: string;
    href: string;
};

export type NavigationBarItem = {
    label: string;
    options: NavigationBarItemOption[];
};
