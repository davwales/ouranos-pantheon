import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Box as MuiBox } from "@mui/material";
import React from "react";

interface BoxProps {
    styling?: StyleProps,
    children?: React.ReactNode,
    role?: string,
    onKeyDown?: (event: React.KeyboardEvent | React.MouseEvent) => void
};

export default function Box(props: BoxProps) {
    return (
        <MuiBox
            sx={props.styling && convertToSx(props.styling)}
            role={props.role}
            onKeyDown={props.onKeyDown}
        >
            {props.children}
        </MuiBox>
    )
}