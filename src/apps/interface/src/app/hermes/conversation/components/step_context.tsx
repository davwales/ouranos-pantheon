import { Textarea } from "@/components/ui/textarea";
import { ChangeEvent } from "react";

export default function StepContext({
    context,
    setContext,
    ...props
}: React.ComponentProps<"div"> & {
    context: string | undefined;
    setContext: (context: string) => void;
}) {
    const handleContextChanged = (event: ChangeEvent<HTMLTextAreaElement>) => {
        setContext(event.target.value);
    };

    return (
        <div {...props}>
            <Textarea value={context} onChange={handleContextChanged} />
        </div>
    );
};
