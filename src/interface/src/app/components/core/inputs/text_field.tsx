import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { TextField as MuiTextField } from "@mui/material";

interface TextFieldProps {
    styling?: StyleProps;
    label?: string;
    placeholder?: string;
    value?: string | number;
    multiline?: boolean;
    rows?: number;
    minRows?: number;
    maxRows?: number;
    variant?: "standard" | "outlined" | "filled";
    fullWidth?: boolean;
    required?: boolean;
    disabled?: boolean;
    margin?: "none" | "dense" | "normal";
    onChange?: (x: string, e: React.ChangeEvent<HTMLInputElement>) => void;
}

export default function TextField(props: TextFieldProps) {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (!props.onChange) return;
        props.onChange(e.target.value, e);
    }

    return (
        <MuiTextField
            label={props.label}
            placeholder={props.placeholder}
            value={props.value}
            multiline={props.multiline}
            rows={props.rows}
            minRows={props.minRows}
            maxRows={props.maxRows}
            variant={props.variant}
            margin={props.margin}
            fullWidth={props.fullWidth}
            required={props.required}
            disabled={props.disabled}
            onChange={handleChange}
            sx={props.styling && convertToSx(props.styling)}
        />
    );
}
