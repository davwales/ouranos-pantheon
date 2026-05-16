import { abbreviateNumber } from "@/components/shared/pretty-number/abbreviate-number";


export function PrettyNumber({
    number,
    decimals,
    ...props
}: React.ComponentProps<"div"> & {
    number: number;
    decimals?: number;
}) {
    return (
        <div {...props}>
            {abbreviateNumber(number, decimals ?? 2)}
        </div>
    );
}