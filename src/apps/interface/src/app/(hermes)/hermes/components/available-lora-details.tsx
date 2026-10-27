import { Typography } from "@/components/shared/typography";
import { type AvailableLora } from "@/lib/api/hermes";

export function AvailableLoraDetails({
  lora,
  identifier,
  catalogueStatus,
}: {
  lora: AvailableLora | undefined;
  identifier: string;
  catalogueStatus: "ready" | "empty" | "error";
}) {
  if (catalogueStatus === "error") {
    return (
      <Typography variant="muted" className="text-destructive">
        Couldn’t load the image server’s LoRAs. Try again later.
      </Typography>
    );
  }

  if (catalogueStatus === "empty") {
    return (
      <Typography variant="muted">
        No LoRAs have been synced from the image server yet.
      </Typography>
    );
  }

  if (!lora) {
    return identifier ? (
      <Typography variant="muted" className="text-destructive">
        {identifier} is no longer on the image server, so it isn’t applied.
      </Typography>
    ) : null;
  }

  return (
    <>
      <Typography variant="muted" className="break-all">
        {lora.identifier}
      </Typography>
      {!lora.isCompatible && (
        <Typography
          variant="muted"
          className="text-amber-700 dark:text-amber-300"
        >
          Made for {lora.compatClass} models, not the configured image model.
          It may have no effect or spoil images.
        </Typography>
      )}
      {lora.usageHint && (
        <Typography variant="muted">{lora.usageHint}</Typography>
      )}
    </>
  );
}
