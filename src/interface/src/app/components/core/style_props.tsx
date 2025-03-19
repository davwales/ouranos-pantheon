import { SxProps } from "@mui/material";

export type SpacingToken = 'none' | 'small' | 'medium' | 'large' | 'xl' | 'xxl' | 'auto';
export type ColorToken = 'primary' | 'secondary' | 'inherit' | 'error' | 'success';
export type FontSizeToken = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl';
export type FontWeightToken = 'light' | 'regular' | 'medium' | 'bold';
export type BorderRadiusToken = 'none' | 'sm' | 'md' | 'lg' | 'full';
export type ShadowToken = 'none' | 'sm' | 'md' | 'lg';

export type StyleProps = Omit<React.CSSProperties, keyof CustomStyleProps> & CustomStyleProps;

/**
 * Comprehensive styling props interface based on standard React styling patterns
 * and design tokens that can easily be converted to any component library
 */
interface CustomStyleProps {
    sx?: SxProps;
    m?: SpacingToken;
    mt?: SpacingToken;
    mr?: SpacingToken;
    mb?: SpacingToken;
    ml?: SpacingToken;
    mx?: SpacingToken;
    my?: SpacingToken;
    p?: SpacingToken;
    pt?: SpacingToken;
    pr?: SpacingToken;
    pb?: SpacingToken;
    pl?: SpacingToken;
    px?: SpacingToken;
    py?: SpacingToken;
    color?: ColorToken;
    bgColor?: ColorToken;
    borderColor?: ColorToken;
    fontSize?: FontSizeToken;
    fontWeight?: FontWeightToken;
    borderRadius?: BorderRadiusToken;
    shadow?: ShadowToken;
    gap?: SpacingToken;
    gridGap?: SpacingToken;
}