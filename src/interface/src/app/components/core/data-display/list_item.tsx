import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { ListItem as MuiListItem } from "@mui/material";

interface ListItemProps {
    children: React.ReactNode;
    onClick?: (event: React.MouseEvent<HTMLLIElement>) => void;
    styling?: StyleProps;
}

export default function ListItem(props: ListItemProps) {
    return (
        <MuiListItem
            sx={props.styling && convertToSx(props.styling)}
            onClick={props.onClick}
        >
            {props.children}
        </MuiListItem>
    );
}