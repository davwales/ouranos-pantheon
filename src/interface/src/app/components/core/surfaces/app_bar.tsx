import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { AppBar as MuiAppBar, Toolbar } from "@mui/material";

interface AppBarProps {
    children: React.ReactNode;
    position?: 'fixed' | 'sticky' | 'absolute' | 'static' | 'relative';
    styling?: StyleProps;
}

export default function AppBar(props: AppBarProps) {
    return (
        <MuiAppBar position={props.position ?? 'sticky'} sx={props.styling && convertToSx(props.styling)}>
            <Toolbar>
                {props.children}
            </Toolbar>
        </MuiAppBar>
    );
}
