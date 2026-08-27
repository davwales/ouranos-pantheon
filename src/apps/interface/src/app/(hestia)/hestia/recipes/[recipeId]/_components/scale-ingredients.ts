export const SCALE_MIN = 0.5;
export const SCALE_MAX = 4;
export const SCALE_STEP = 0.5;

const FRACTIONS: ReadonlyArray<{ value: number; glyph: string }> = [
  { value: 0.25, glyph: "¼" },
  { value: 1 / 3, glyph: "⅓" },
  { value: 0.5, glyph: "½" },
  { value: 2 / 3, glyph: "⅔" },
  { value: 0.75, glyph: "¾" },
];

const FRACTION_TOLERANCE = 0.02;

export function clampScale(value: number): number {
  const clamped = Math.min(Math.max(value, SCALE_MIN), SCALE_MAX);
  return Math.round(clamped / SCALE_STEP) * SCALE_STEP;
}

export function scaleQuantity(quantity: number, scale: number): number {
  return quantity * scale;
}

export function formatQuantity(value: number): string {
  if (!Number.isFinite(value) || value <= 0) {
    return "";
  }

  const rounded = Math.round(value);
  if (Math.abs(value - rounded) < FRACTION_TOLERANCE) {
    return String(rounded);
  }

  const whole = Math.floor(value);
  const remainder = value - whole;
  const match = FRACTIONS.find(
    (fraction) => Math.abs(remainder - fraction.value) < FRACTION_TOLERANCE,
  );

  if (!match) {
    return trimTrailingZeros(value.toFixed(2));
  }

  if (whole === 0) {
    return match.glyph;
  }

  return `${whole}${match.glyph}`;
}

function trimTrailingZeros(formatted: string): string {
  if (!formatted.includes(".")) {
    return formatted;
  }

  return formatted.replace(/\.?0+$/, "");
}
