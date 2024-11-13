import { Box, SxProps } from "@mui/material";

interface PrettyNumberProps {
    number: number
    decimals?: number,
    sx?: SxProps
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
    if(!number) {
        return "0";
    }

    for(const a of abbreviationMapping) {
        if(number >= a.threshold) {
            const formattedNumber = (number / a.threshold).toFixed(decimals);
            return `${formattedNumber}${a.abbreviation}`;
        }
    }
    return number.toFixed(decimals);
};

export function PrettyNumber(props: PrettyNumberProps) {
    return (
        <Box sx={props.sx}>
            {abbreviateNumber(props.number, props.decimals ?? 2)}
        </Box>
    );
}