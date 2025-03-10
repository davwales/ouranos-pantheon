import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { ListItemIcon as MuiListItemIcon } from "@mui/material";

interface ListItemIconProps {
    children: React.ReactNode;
    styling?: StyleProps;
}

export default function ListItemIcon(props: ListItemIconProps) {
    return (
        <MuiListItemIcon sx={props.styling && convertToSx(props.styling)}>
            {props.children}
        </MuiListItemIcon>
    );
}
