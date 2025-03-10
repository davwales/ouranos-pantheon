import { Snackbar as MuiSnackbar } from "@mui/material";
import React from "react";

interface SnackbarProps {
    children: React.ReactElement;
    open: boolean;
    anchorOrigin: {
        vertical: 'top' | 'bottom';
        horizontal: 'left' | 'right';
    };
    onClose: () => void;
}

export default function Snackbar(props: SnackbarProps) {
    return (
        <MuiSnackbar
            open={props.open}
            anchorOrigin={props.anchorOrigin}
            onClose={props.onClose}
            autoHideDuration={6000}
        >
            {props.children}
        </MuiSnackbar>
    );
}