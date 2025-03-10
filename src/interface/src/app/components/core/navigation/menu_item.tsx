import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { MenuItem as MuiMenuItem } from "@mui/material";
import React from "react";

interface MenuItemProps {
    children: React.ReactNode;
    value?: any;
    component?: React.ElementType;
    onClick?: () => void;
    styling?: StyleProps;
}

export default function MenuItem(props: MenuItemProps) {
    return (
        <MuiMenuItem
            sx={props.styling && convertToSx(props.styling)}
            value={props.value}
            onClick={props.onClick}
            component={props.component ?? 'div'}
        >
            {props.children}
        </MuiMenuItem>
    );
}