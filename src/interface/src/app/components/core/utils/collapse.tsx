import { Collapse as MuiCollapse } from "@mui/material";

interface CollapseProps {
    children: React.ReactNode;
    in?: boolean;
}

export default function Collapse(props: CollapseProps) {
    return (
        <MuiCollapse in={props.in} timeout='auto' unmountOnExit>
            {props.children}
        </MuiCollapse>
    );
}
