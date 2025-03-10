import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Card as MuiCard } from "@mui/material";

type CardVariant = "outlined" | "elevation";

interface CardProps {
    children: React.ReactNode;
    styling?: StyleProps;
    onClick?: () => void;
    variant?: CardVariant;
}

export default function Card(props: CardProps) {
    return (
        <MuiCard
            variant={props.variant}
            onClick={props.onClick}
            sx={props.styling && convertToSx(props.styling)}
        >
            {props.children}
        </MuiCard>
    );
}