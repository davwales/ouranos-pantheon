import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Tooltip as MuiTooltip } from "@mui/material";
import React from "react";

interface TooltipProps {
    title: React.ReactNode;
    children: React.ReactNode;
    styling?: StyleProps;
}

export default function Tooltip(props: TooltipProps) {
    return (
        <MuiTooltip title={props.title} sx={props.styling && convertToSx(props.styling)}>
            <div>
                {props.children}
            </div>
        </MuiTooltip>
    );
}