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
        name: "One Hour",
        seconds: hourSeconds
    },
    {
        name: "Four Hours",
        seconds: 4 * hourSeconds
    },
    {
        name: "One Day",
        seconds: daySeconds
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
        name: "Max",
        seconds: -1
    }
];