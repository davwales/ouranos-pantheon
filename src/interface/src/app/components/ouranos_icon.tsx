import { Avatar, SxProps } from "@mui/material";

interface OuranosIconProps {
    iconSize?: number,
    sx?: SxProps
};

export default function OuranosIcon(props: OuranosIconProps) {
    return (
        <Avatar
            alt="Ouranos"
            src="/ouranos_icon.png"
            sx={{
                ...props.sx,
                width: props.iconSize || 50,
                height: props.iconSize || 50
            }}
        />
    );
};