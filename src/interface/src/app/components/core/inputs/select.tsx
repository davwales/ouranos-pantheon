import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Select as MuiSelect } from "@mui/material";
import React from "react";

export type SelectChangeEvent = React.ChangeEvent<HTMLInputElement> | (Event & {
    target: {
        value: any;
        name: string;
    };
})

interface SelectProps {
    value: any;
    children: React.ReactNode;
    label?: string;
    variant?: 'filled' | 'outlined' | 'standard';
    onChange?: (event: SelectChangeEvent) => void;
    styling?: StyleProps;
}

export function Select(props: SelectProps) {
    return (
        <MuiSelect
            label={props.label}
            value={props.value}
            variant={props.variant}
            onChange={props.onChange}
            sx={props.styling && convertToSx(props.styling)}
        >
            {props.children}
        </MuiSelect>
    );
}
