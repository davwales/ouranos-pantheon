export interface NavigationBarItemOption {
    label: string;
    description?: string;
    href: string;
}

export interface NavigationBarItem {
    label: string;
    options: NavigationBarItemOption[];
}
