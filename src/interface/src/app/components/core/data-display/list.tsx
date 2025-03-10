import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { List as MuiList } from "@mui/material";

interface ListProps {
    children: React.ReactNode;
    styling?: StyleProps;
    component?: React.ElementType;
    disablePadding?: boolean;
}

export default function List(props: ListProps) {
    return (
        <MuiList
            component={props.component ?? 'div'}
            sx={props.styling && convertToSx(props.styling)}
            disablePadding={props.disablePadding}
        >
            {props.children}
        </MuiList>
    );
}
