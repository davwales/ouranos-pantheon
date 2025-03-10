import { convertToSx } from "@/app/components/core/mui_style_resolvers"
import { StyleProps } from "@/app/components/core/style_props"
import { Box as MuiBox } from "@mui/material"

interface FormBoxProps {
    styling?: StyleProps,
    children?: React.ReactNode,
    role?: string,
    onSubmit?: (event: React.FormEvent<HTMLFormElement>) => void
};

export default function FormBox(props: FormBoxProps) {
    return (
        <MuiBox
            sx={props.styling && convertToSx(props.styling)}
            role={props.role}
            component="form"
            onSubmit={props.onSubmit}
        >
            {props.children}
        </MuiBox>
    )
}