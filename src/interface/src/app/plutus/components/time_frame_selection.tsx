import { Select, SelectChangeEvent } from "@/app/components/core/inputs/select";
import MenuItem from "@/app/components/core/navigation/menu_item";
import { StyleProps } from "@/app/components/core/style_props";
import { timeFrames } from "@/app/plutus/constants/time_frames";

interface TimeFrameSelectionProps {
    seconds: number,
    onChange: (seconds: number) => void,
    styling?: StyleProps
};

export default function TimeFrameSelection(props: TimeFrameSelectionProps) {
    const handleScopeChange = (event: SelectChangeEvent) => {
        if (!props.onChange) {
            return;
        }
        props.onChange(event.target.value as number);
    };

    return (
        <Select
            variant="standard"
            value={props.seconds}
            onChange={handleScopeChange}
            styling={{ ...props.styling, minWidth: "10rem" }}
        >
            {timeFrames.map(t => (
                <MenuItem key={t.name} value={t.seconds}>{t.name}</MenuItem>
            ))}
        </Select>
    );
}