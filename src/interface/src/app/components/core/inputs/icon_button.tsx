import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { ColorToken, StyleProps } from "@/app/components/core/style_props";
import { IconButton as MuiIconButton } from "@mui/material";

interface IconButtonProps {
    children: React.ReactNode;
    styling?: StyleProps;
    color?: ColorToken;
    edge?: "start" | "end" | false;
    disabled?: boolean;
    onClick?: (event: React.KeyboardEvent | React.MouseEvent) => void;
}

export default function IconButton(props: IconButtonProps) {
    return (
        <MuiIconButton
            color={props.color}
            edge={props.edge}
            disabled={props.disabled}
            onClick={props.onClick}
            sx={props.styling && convertToSx(props.styling)}
        >
            {props.children}
        </MuiIconButton>
    );
}