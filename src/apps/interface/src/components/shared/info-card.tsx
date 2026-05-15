import { Typography } from "@/components/shared/typography";
import { cn } from "@/lib/utils";
import Image from "next/image";

export default function InfoCard({
  label,
  description,
  iconSrc,
  alt,
  className,
  children,
  ...props
}: React.ComponentProps<"div"> & {
  label: string;
  description?: string | null | undefined;
  iconSrc?: string | null | undefined;
  alt?: string;
}) {
  return (
    <div
      {...props}
      className={cn(
        "flex items-start rounded-4xl border-2 border-accent py-4 px-3",
        className,
      )}
    >
      {iconSrc && (
        <div className="shrink-0 w-20 h-20 bg-accent-foreground rounded-2xl flex items-center justify-center">
          <Image
            src={iconSrc}
            alt={alt ?? ""}
            width={80}
            height={80}
            className="w-fit rounded-xl"
          />
        </div>
      )}
      <div className="ml-4">
        <Typography variant="small">{label}</Typography>
        {description && (
          <Typography variant="muted" className="line-clamp-3">
            {description}
          </Typography>
        )}
        {children}
      </div>
    </div>
  );
}
