export interface NavigationBarItemOption {
    label: string;
    href: string;
}

export interface NavigationBarItem {
    label: string;
    options: NavigationBarItemOption[];
}