export const minuteSeconds = 60;
export const hourSeconds = 60 * minuteSeconds;
export const daySeconds = 24 * hourSeconds;
export const monthSeconds = 30 * daySeconds;
export const yearSeconds = 365 * daySeconds;

export interface TimeFrame {
    name: string,
    seconds: number
};

export const timeFrames: TimeFrame[] = [
    {
        name: "Fifteen Minutes",
        seconds: 15 * minuteSeconds
    },
    {
        name: "Thirty Minutes",
        seconds: 30 * minuteSeconds
    },
    {
        name: "One Hour",
        seconds: hourSeconds
    },
    {
        name: "Three Hours",
        seconds: 3 * hourSeconds
    },
    {
        name: "Six Hours",
        seconds: 6 * hourSeconds
    },
    {
        name: "Twelve Hours",
        seconds: 12 * hourSeconds
    },
    {
        name: "One Day",
        seconds: daySeconds
    },
    {
        name: "Three Days",
        seconds: 3 * daySeconds
    },
    {
        name: "Five Days",
        seconds: 5 * daySeconds
    },
    {
        name: "One Week",
        seconds: 7 * daySeconds
    },
    {
        name: "One Month",
        seconds: 30 * daySeconds
    },
    {
        name: "Six Months",
        seconds: 6 * monthSeconds
    },
    {
        name: "One Year",
        seconds: yearSeconds
    },
    {
        name: "All Time",
        seconds: -1
    }
];