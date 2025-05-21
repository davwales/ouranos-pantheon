import { timeFrames } from "@/app/plutus/constants/time_frames";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

export default function TimeFrameSelection({
  triggerClassName,
  ...props
}: React.ComponentProps<"div"> & {
  triggerClassName?: string;
}) {
  const [timeFrameSeconds, setTimeFrameSeconds] = usePlutusStore(
    (state: PlutusState) => [state.timeFrameSeconds, state.setTimeFrameSeconds]
  );

  const handleValueChanged = (value: string) => {
    const parsedSeconds = parseInt(value, 10);
    if (isNaN(parsedSeconds)) {
      return;
    }

    setTimeFrameSeconds(parsedSeconds);
  };

  return (
    <div {...props}>
      <Select
        onValueChange={handleValueChanged}
        defaultValue={String(timeFrameSeconds)}
      >
        <SelectTrigger className={`w-50 ${triggerClassName}`}>
          <SelectValue placeholder="Seconds" />
        </SelectTrigger>
        <SelectContent>
          {timeFrames.map((t) => (
            <SelectItem key={t.name} value={String(t.seconds)}>
              {t.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
