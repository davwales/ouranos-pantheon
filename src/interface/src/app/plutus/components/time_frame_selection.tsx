import { timeFrames } from "@/app/plutus/constants/time_frames";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

export default function TimeFrameSelection({
    seconds,
    onValueChange,
    triggerClassName,
    ...props
}: React.ComponentProps<"div"> & {
    seconds: number,
    onValueChange: (seconds: number) => void,
    className?: string;
    triggerClassName?: string;
}) {
    const handleValueChanged = (value: string) => {
        const parsedSeconds = parseInt(value, 10);
        if (isNaN(parsedSeconds)) {
            return;
        }

        onValueChange(parsedSeconds);
    };

    return (
        <div {...props}>
            <Select onValueChange={handleValueChanged} defaultValue={String(seconds)}>
                <SelectTrigger className={`w-50 ${triggerClassName}`}>
                    <SelectValue placeholder="Seconds" />
                </SelectTrigger>
                <SelectContent>
                    {timeFrames.map(t => (
                        <SelectItem key={t.name} value={String(t.seconds)}>{t.name}</SelectItem>
                    ))}
                </SelectContent>
            </Select>
        </div>
    );
}