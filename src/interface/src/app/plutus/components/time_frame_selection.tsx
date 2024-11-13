import { MenuItem, Select, SelectChangeEvent, SxProps } from "@mui/material";
import { timeFrames } from "../constants/time_frames";

interface TimeFrameSelectionProps {
    seconds: number,
    onChange: (seconds: number) => void,
    sx?: SxProps
};

export default function TimeFrameSelection(props: TimeFrameSelectionProps) {
    const handleScopeChange = (event: SelectChangeEvent<number>) => {
        if (!props.onChange) {
            return;
        }
        props.onChange(event.target.value as number);
    };

    return (
        <Select variant="standard" value={props.seconds} onChange={handleScopeChange} sx={{ ...props.sx, minWidth: "10rem" }}>
            {timeFrames.map(t => (
                <MenuItem key={t.name} value={t.seconds}>{t.name}</MenuItem>
            ))}
        </Select>
    );
}