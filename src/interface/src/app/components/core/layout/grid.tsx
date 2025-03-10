import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Grid2 } from "@mui/material";

interface GridProps {
    children: React.ReactNode;
    styling?: StyleProps;
    container?: boolean;
    spacing?: number;
    size?: number | {
        xs?: number;
        sm?: number;
        md?: number;
        lg?: number;
        xl?: number;
    }
}

export default function Grid(props: GridProps) {
    return (
        <Grid2
            container={props.container}
            spacing={props.spacing}
            size={props.size}
            sx={props.styling && convertToSx(props.styling)}
        >
            {props.children}
        </Grid2>
    );
}