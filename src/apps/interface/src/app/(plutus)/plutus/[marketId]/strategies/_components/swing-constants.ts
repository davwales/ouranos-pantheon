import { type SwingConfiguration } from "@/lib/api/plutus";
import { INPUT_KINDS } from "./strategy-constants";

export const DEFAULT_SWING_CONFIGURATION: SwingConfiguration = {
  mode: "Swing",
  inputWeights: INPUT_KINDS.map((kind) => ({ kind, weight: 1 })),
  buyThreshold: null,
  sellThreshold: null,
  maxPositions: 10,
  maxPositionPercent: 0.2,
  holdPeriodDays: 7,
};

export function withAllInputKinds(
  configuration: SwingConfiguration,
): SwingConfiguration {
  return {
    ...configuration,
    inputWeights: INPUT_KINDS.map((kind) => ({
      kind,
      weight:
        configuration.inputWeights.find((w) => w.kind === kind)?.weight ?? 0,
    })),
  };
}

export function withoutZeroWeights(
  configuration: SwingConfiguration,
): SwingConfiguration {
  return {
    ...configuration,
    inputWeights: configuration.inputWeights.filter((w) => w.weight !== 0),
  };
}
