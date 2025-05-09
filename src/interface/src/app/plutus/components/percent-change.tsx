import { abbreviateNumber } from "@/app/components/pretty-number";
import { Typography } from "@/app/components/typography";
import { Tooltip, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { TooltipContent } from "@radix-ui/react-tooltip";
import { MoveRight, TrendingDown, TrendingUp } from "lucide-react";
import { useMemo } from "react";

export default function PercentChange({
    label,
    current,
    previous,
    className,
    ...props
}: React.ComponentProps<"div"> & {
    label?: string;
    current: number | null | undefined;
    previous: number | null | undefined;
}) {
    const calculatePercentChange = (): number => {
        if (!previous || !current) {
            return 0;
        }

        if (previous === 0) {
            return current > 0 ? 100 : -100;
        }

        return ((current - previous) / Math.abs(previous)) * 100;
    };

    const getIcon = (change: number): React.ReactNode => {
        if (change < 0) {
            return <TrendingDown color='red' />
        } else if (change > 0) {
            return <TrendingUp color='green' />
        } else {
            return <MoveRight color='blue' />
        }
    };

    const percentChange = calculatePercentChange();
    const icon = getIcon(percentChange);
    const formattedPercent = Math.abs(percentChange).toFixed(2);

    const TooltipItem = ({
        label,
        value,
        valueClassName
    }: {
        label: string,
        value: number | null | undefined,
        valueClassName?: string;
    }) => (
        <div className="flex gap-2 justify-between items-center">
            <Typography variant="muted">{label}</Typography>
            <Typography variant="small" className={valueClassName}>
                {value ? abbreviateNumber(value) : "-"}
            </Typography>
        </div>
    );

    const difference = useMemo(() => (current || 0) - (previous || 0), [current, previous]);

    return (
        <TooltipProvider>
            <Tooltip>
                <TooltipTrigger>
                    <div className={`flex gap-2 ${className}`} {...props}>
                        {icon}
                        <Typography variant="muted">
                            {formattedPercent}% {label}
                        </Typography>
                    </div>
                </TooltipTrigger>
                <TooltipContent className="bg-background border p-2 rounded">
                    <TooltipItem label="Previous" value={previous} />
                    <TooltipItem label={label || "Current"} value={current} />
                    <TooltipItem
                        label="Difference"
                        value={difference}
                        valueClassName={difference > 0 ? "text-green-600" : "text-red-500"}
                    />
                </TooltipContent>
            </Tooltip>
        </TooltipProvider >
    );
}
