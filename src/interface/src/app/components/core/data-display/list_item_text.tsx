import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { ListItemText as MuiListItemText } from "@mui/material";

interface ListItemTextProps {
    primary: React.ReactNode;
    secondary?: React.ReactNode;
    styling?: StyleProps;
}

export default function ListItemText(props: ListItemTextProps) {
    return (
        <MuiListItemText
            primary={props.primary}
            secondary={props.secondary}
            sx={props.styling && convertToSx(props.styling)}
        />
    );
}