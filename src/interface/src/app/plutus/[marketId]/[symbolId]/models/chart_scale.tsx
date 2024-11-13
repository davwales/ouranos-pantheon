export const allScales = ["band", "linear", "log", "point", "pow", "sqrt", "time", "utc"] as const;
export type ChartScale = typeof allScales[number];