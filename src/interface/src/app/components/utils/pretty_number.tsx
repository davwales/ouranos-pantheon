import Box from "@/app/components/core/layout/box";
import { StyleProps } from "@/app/components/core/style_props";

interface PrettyNumberProps {
    number: number
    decimals?: number,
    styling?: StyleProps
};

interface NumberAbbreviation {
    threshold: number,
    abbreviation: string
};

const abbreviationMapping: NumberAbbreviation[] = [
    {
        threshold: 1000000000,
        abbreviation: "B"
    },
    {
        threshold: 1000000,
        abbreviation: "M"
    },
    {
        threshold: 1000,
        abbreviation: "K"
    }
];

export const abbreviateNumber = (number: number, decimals: number = 2) => {
    if (!number) {
        return "0";
    }

    for (const a of abbreviationMapping) {
        if (number >= a.threshold) {
            const formattedNumber = (number / a.threshold).toFixed(decimals);
            return `${formattedNumber}${a.abbreviation}`;
        }
    }
    return number.toFixed(decimals);
};

export function PrettyNumber(props: PrettyNumberProps) {
    return (
        <Box styling={props.styling}>
            {abbreviateNumber(props.number, props.decimals ?? 2)}
        </Box>
    );
}