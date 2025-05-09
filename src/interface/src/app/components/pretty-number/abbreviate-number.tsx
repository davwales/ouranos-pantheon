const abbreviationMapping: { threshold: number, abbreviation: string }[] = [
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

    const absNumber = Math.abs(number);
    for (const a of abbreviationMapping) {
        if (absNumber >= a.threshold) {
            const formattedNumber = (number / a.threshold).toFixed(decimals);
            return `${formattedNumber}${a.abbreviation}`;
        }
    }
    return number.toFixed(decimals);
};