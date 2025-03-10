import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Paper as MuiPaper } from "@mui/material";
import React from "react";

interface PaperProps {
    children: React.ReactNode;
    styling?: StyleProps;
}

export default function Paper(props: PaperProps) {
    return (
        <MuiPaper sx={props.styling && convertToSx(props.styling)}>
            {props.children}
        </MuiPaper>
    );
}