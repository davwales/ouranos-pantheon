import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { TextField as MuiTextField } from "@mui/material";

interface NumberFieldProps {
    styling?: StyleProps;
    label?: string;
    placeholder?: string;
    value?: number;
    multiline?: boolean;
    rows?: number;
    variant?: "standard" | "outlined" | "filled";
    fullWidth?: boolean;
    required?: boolean;
    disabled?: boolean;
    margin?: "none" | "dense" | "normal";
    onChange?: (x: number, e: React.ChangeEvent<HTMLInputElement>) => void;
}

export default function NumberField(props: NumberFieldProps) {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (!props.onChange) return;

        const value = parseFloat(e.target.value);
        props.onChange(value, e);
    };

    return (
        <MuiTextField
            type="number"
            label={props.label}
            placeholder={props.placeholder}
            value={props.value}
            multiline={props.multiline}
            rows={props.rows}
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
