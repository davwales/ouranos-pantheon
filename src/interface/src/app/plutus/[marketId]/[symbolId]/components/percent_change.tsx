import Tooltip from "@/app/components/core/data-display/tooltip";
import Typography from "@/app/components/core/data-display/typography";
import TrendingDown from "@/app/components/core/icons/trending_down";
import TrendingFlat from "@/app/components/core/icons/trending_flat";
import TrendingUp from "@/app/components/core/icons/trending_up";
import Box from "@/app/components/core/layout/box";
import { StyleProps } from "@/app/components/core/style_props";
import { PrettyNumber } from "@/app/components/utils/pretty_number";

interface PercentChangeProps {
    label?: string;
    current?: number;
    previous?: number;
    styling?: StyleProps;
}

export default function PercentChange(props: PercentChangeProps) {
    const calculatePercentChange = (): number => {
        if (!props.previous || !props.current) {
            return 0;
        }

        if (props.previous === 0) {
            return props.current > 0 ? 100 : -100;
        }

        return ((props.current - props.previous) / Math.abs(props.previous)) * 100;
    };

    const getIcon = (change: number): React.ReactNode => {
        if (change < 0) {
            return <TrendingDown color='error' />
        } else if (change > 0) {
            return <TrendingUp color='success' />
        } else {
            return <TrendingFlat color='primary' />
        }
    };

    const percentChange = calculatePercentChange();
    const icon = getIcon(percentChange);
    const formattedPercent = Math.abs(percentChange).toFixed(2);

    const tooltip = (
        <>
            Previous: {props.previous ? <PrettyNumber number={props.previous} /> : "-"}
            Current: {props.current ? <PrettyNumber number={props.current} /> : "-"}
        </>
    );

    return (
        <Tooltip title={tooltip}>
            <Box styling={{ display: 'flex', gap: 'small', ...props.styling }}>
                {icon}
                <Typography variant="caption">{formattedPercent}%</Typography>
                {props.label && (<Typography variant="caption">{props.label}</Typography>)}
            </Box>
        </Tooltip>
    );
}
