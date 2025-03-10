import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Menu as MuiMenu } from "@mui/material";

interface MenuProps {
    children: React.ReactNode;
    open: boolean;
    onClose?: () => void;
    anchorEl?: HTMLElement;
    styling?: StyleProps;
}

export default function Menu(props: MenuProps) {
    return (
        <MuiMenu
            sx={props.styling && convertToSx(props.styling)}
            open={props.open}
            anchorEl={props.anchorEl}
            onClose={props.onClose}
        >
            {props.children}
        </MuiMenu>
    );
}