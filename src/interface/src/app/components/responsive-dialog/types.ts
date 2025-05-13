export interface DialogProps {
    title: string;
    description: string;
    trigger: React.ReactNode;
    children: React.ReactNode;
    open?: boolean;
    onOpenChange?: (open: boolean) => void;
}
