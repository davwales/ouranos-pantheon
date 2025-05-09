import { Textarea } from "@/components/ui/textarea";

export default function AutosizeTextarea(props: React.ComponentProps<typeof Textarea>) {
    return (
        <Textarea
            onInput={(e) => {
                const target = e.target as HTMLTextAreaElement;
                target.style.height = '0px';
                target.style.height = target.scrollHeight + 'px';
            }}
            {...props}
        />
    );
}