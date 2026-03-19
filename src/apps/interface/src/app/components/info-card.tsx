import { Typography } from "@/app/components/typography";

export default function InfoCard({
    label,
    description,
    iconSrc,
    className,
    ...props
}: React.ComponentProps<"div"> & {
    label: string;
    description?: string | null | undefined;
    iconSrc?: string | null | undefined;
}) {
    return (
        <div {...props} className={`flex items-center rounded-4xl border-2 border-accent py-4 px-3 ${className}`}>
            {iconSrc && (
                <div className="flex-shrink-0 w-20 h-20 bg-accent-foreground rounded-2xl flex items-center justify-center">
                    <img src={iconSrc} className="w-fit rounded-xl" />
                </div>
            )}
            <div className="ml-4">
                <Typography variant="small">{label}</Typography>
                {description && <Typography variant="muted">{description}</Typography>}
            </div>
        </div>
    );
}