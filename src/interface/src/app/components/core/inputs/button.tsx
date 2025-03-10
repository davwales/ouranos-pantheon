import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { ColorToken, StyleProps } from "@/app/components/core/style_props";
import { Button as MuiButton } from "@mui/material";

type ButtonVariant = 'contained' | 'outlined' | 'text';

interface ButtonProps {
    styling?: StyleProps;
    children?: React.ReactNode;
    component?: React.ElementType;
    onClick?: (event: React.MouseEvent<HTMLButtonElement>) => void;
    startIcon?: React.ReactNode;
    endIcon?: React.ReactNode;
    variant?: ButtonVariant;
    disabled?: boolean;
    color?: ColorToken;
    href?: string;
    submit?: boolean;
}

export default function Button(props: ButtonProps) {
    return (
        <MuiButton
            sx={props.styling && convertToSx(props.styling)}
            color={props.color}
            component={props.component ?? 'button'}
            onClick={props.onClick}
            startIcon={props.startIcon}
            endIcon={props.endIcon}
            variant={props.variant ?? 'text'}
            disabled={props.disabled}
            href={props.href}
            type={props.submit && 'submit'}
        >
            {props.children}
        </MuiButton>
    );
}