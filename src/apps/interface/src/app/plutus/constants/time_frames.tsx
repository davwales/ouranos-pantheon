export type TimeFrameKey =
  | "FifteenMinutes"
  | "OneHour"
  | "FourHours"
  | "OneDay"
  | "OneWeek"
  | "OneMonth"
  | "SixMonths"
  | "OneYear"
  | "AllTime";

export interface TimeFrame {
  key: TimeFrameKey;
  name: string;
}

export const timeFrames: TimeFrame[] = [
  { key: "FifteenMinutes", name: "15 Minutes" },
  { key: "OneHour", name: "One Hour" },
  { key: "FourHours", name: "Four Hours" },
  { key: "OneDay", name: "One Day" },
  { key: "OneWeek", name: "One Week" },
  { key: "OneMonth", name: "One Month" },
  { key: "SixMonths", name: "Six Months" },
  { key: "OneYear", name: "One Year" },
  { key: "AllTime", name: "All Time" },
];
