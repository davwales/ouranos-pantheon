import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { ColorToken, StyleProps } from "@/app/components/core/style_props";
import { AppBar as MuiAppBar, Toolbar } from "@mui/material";

interface AppBarProps {
    children: React.ReactNode;
    position?: 'fixed' | 'sticky' | 'absolute' | 'static' | 'relative';
    color?: ColorToken;
    styling?: StyleProps;
}

export default function AppBar(props: AppBarProps) {
    return (
        <MuiAppBar
            position={props.position ?? 'sticky'}
            color={props.color || 'primary'}
            sx={props.styling && convertToSx(props.styling)}
        >
            <Toolbar>
                {props.children}
            </Toolbar>
        </MuiAppBar>
    );
}
