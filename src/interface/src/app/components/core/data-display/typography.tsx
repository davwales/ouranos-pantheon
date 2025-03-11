import { convertToSx } from '@/app/components/core/mui_style_resolvers';
import { StyleProps } from '@/app/components/core/style_props';
import { Typography as MuiTypography } from '@mui/material';

type TypographyVariant = 'body1' | 'h1' | 'h2' | 'h3' | 'h4' | 'h5' | 'h6';

interface TypographyProps {
    children: React.ReactNode;
    variant?: TypographyVariant;
    styling?: StyleProps;
    gutterBottom?: boolean;
}

export default function Typography(props: TypographyProps) {
    return (
        <MuiTypography
            variant={props.variant ?? 'body1'}
            sx={props.styling && convertToSx(props.styling)}
            gutterBottom={props.gutterBottom}
        >
            {props.children}
        </MuiTypography>
    );
}