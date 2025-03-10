import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Alert as MuiAlert } from "@mui/material";

interface AlertProps {
    children: React.ReactNode;
    severity?: 'error' | 'warning' | 'info' | 'success';
    onClose?: () => void;
    styling?: StyleProps;
}

export default function Alert(props: AlertProps) {
    return (
        <MuiAlert
            severity={props.severity}
            onClose={props.onClose}
            sx={props.styling && convertToSx(props.styling)}
        >
            {props.children}
        </MuiAlert>
    );
}
