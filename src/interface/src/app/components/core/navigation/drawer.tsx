import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Drawer as MuiDrawer } from "@mui/material";

interface DrawerProps {
    children: React.ReactNode;
    anchor: 'top' | 'right' | 'bottom' | 'left';
    open: boolean;
    onClose?: (event: React.KeyboardEvent | React.MouseEvent) => void;
    styling?: StyleProps;
    paperStyling?: StyleProps;
}

export default function Drawer(props: DrawerProps) {
    return (
        <MuiDrawer
            anchor={props.anchor}
            open={props.open}
            onClose={props.onClose}
            PaperProps={{ sx: props.paperStyling && convertToSx(props.paperStyling) }}
            sx={props.styling && convertToSx(props.styling)}
        >
            {props.children}
        </MuiDrawer>
    );
}