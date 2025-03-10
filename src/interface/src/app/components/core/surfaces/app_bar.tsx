import { AppBar as MuiAppBar, Toolbar } from "@mui/material";

interface AppBarProps {
    children: React.ReactNode;
}

export default function AppBar(props: AppBarProps) {
    return (
        <MuiAppBar position="sticky">
            <Toolbar>
                {props.children}
            </Toolbar>
        </MuiAppBar>
    );
}
