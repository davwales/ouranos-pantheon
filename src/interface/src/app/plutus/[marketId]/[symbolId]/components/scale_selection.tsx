import { Box, MenuItem, Select, SelectChangeEvent, SxProps, Typography } from "@mui/material";
import { ChartScale, allScales } from "../models/chart_scale";

interface ScaleSelectionProps {
    label: string,
    scale: ChartScale,
    onChange: (scale: ChartScale) => void,
    availableScales?: ChartScale[],
    sx?: SxProps
};

export default function ScaleSelection(props: ScaleSelectionProps) {
    const listScales = props.availableScales ?? allScales;

    const handleChange = (event: SelectChangeEvent<ChartScale>) => {
        if (!event.target.value) {
            return;
        }
        props.onChange(event.target.value as ChartScale);
    };

    const scaleDisplay = (scale: ChartScale) => {
        return scale.charAt(0).toUpperCase() + scale.slice(1);
    };

    return (
        <Box sx={{ ...props.sx, display: "flex", my: "1rem" }}>
            <Typography sx={{ m: "auto", paddingRight: "0.5rem" }}>{props.label}:</Typography>
            <Select variant="standard" value={props.scale} onChange={handleChange} sx={{ ...props.sx, minWidth: "10rem" }}>
                {listScales.map(s => (
                    <MenuItem key={s} value={s}>{scaleDisplay(s)}</MenuItem>
                ))}
            </Select>
        </Box>
    );
}