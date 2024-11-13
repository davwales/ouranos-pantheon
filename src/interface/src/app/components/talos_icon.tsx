import { Avatar, SxProps } from "@mui/material";

interface TalosIconProps {
    iconSize?: number,
    sx?: SxProps
};

export default function TalosIcon(props: TalosIconProps) {
    return (
        <Avatar
            alt="Talos"
            src="/talos_icon.png"
            sx={{
                ...props.sx,
                width: props.iconSize || 50,
                height: props.iconSize || 50
            }}
        />
    );
};