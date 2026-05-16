import { TimeFrameKey, timeFrames } from "@/app/(plutus)/plutus/constants/time-frames";
import { PlutusState, usePlutusStore } from "@/stores/plutus-store";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useShallow } from "zustand/react/shallow";

export default function TimeFrameSelection({
  triggerClassName,
  ...props
}: React.ComponentProps<"div"> & {
  triggerClassName?: string;
}) {
  const [timeFrameKey, setTimeFrameKey] = usePlutusStore(
    useShallow((state: PlutusState) => [
      state.timeFrameKey,
      state.setTimeFrameKey,
    ]),
  );

  const handleValueChanged = (value: string) => {
    setTimeFrameKey(value as TimeFrameKey);
  };

  return (
    <div {...props}>
      <Select onValueChange={handleValueChanged} defaultValue={timeFrameKey}>
        <SelectTrigger className={`w-50 ${triggerClassName}`}>
          <SelectValue placeholder="Time Frame" />
        </SelectTrigger>
        <SelectContent>
          {timeFrames.map((t) => (
            <SelectItem key={t.key} value={t.key}>
              {t.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
