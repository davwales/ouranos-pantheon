import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { ListItemButton as MuiListItemButton } from "@mui/material";

interface ListItemButtonProps {
    children: React.ReactNode;
    onClick?: (event: React.KeyboardEvent | React.MouseEvent) => void;
    styling?: StyleProps;
    component?: React.ElementType;
}

export default function ListItemButton(props: ListItemButtonProps) {
    return (
        <MuiListItemButton
            sx={props.styling && convertToSx(props.styling)}
            onClick={props.onClick}
            component={props.component ?? 'div'}
        >
            {props.children}
        </MuiListItemButton>
    );
}