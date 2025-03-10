import { BorderRadiusToken, ColorToken, FontSizeToken, FontWeightToken, ShadowToken, SpacingToken, StyleProps } from '@/app/components/core/style_props';
import { SxProps, Theme } from '@mui/material/styles';

export const resolveSpacing = (token?: SpacingToken): number | string => {
    if (!token) return 0;

    const spacingMap: Record<SpacingToken, number | string> = {
        none: 0,
        small: 1,
        medium: 2,
        large: 4,
        xl: 7,
        xxl: 10,
        auto: 'auto'
    };

    return spacingMap[token];
};

export const resolveColor = (token?: ColorToken, theme?: Theme): string => {
    if (!token) return 'inherit';

    // TODO: use theme.palette
    return token;
};

export const resolveBorderRadius = (token?: BorderRadiusToken): string | number => {
    if (!token) return 0;

    const radiusMap: Record<BorderRadiusToken, string | number> = {
        none: 0,
        sm: '0.125rem',
        md: '0.25rem',
        lg: '0.5rem',
        full: '9999px'
    };

    return radiusMap[token];
};

export const resolveFontSize = (token?: FontSizeToken): string | number => {
    if (!token) return 'inherit';

    const fontSizeMap: Record<FontSizeToken, string> = {
        xs: '0.75rem',
        sm: '0.875rem',
        md: '1rem',
        lg: '1.125rem',
        xl: '1.25rem',
        '2xl': '1.5rem'
    };

    return fontSizeMap[token];
};

export const resolveFontWeight = (token?: FontWeightToken): number => {
    if (!token) return 400;

    const weightMap: Record<FontWeightToken, number> = {
        light: 300,
        regular: 400,
        medium: 500,
        bold: 700
    };

    return weightMap[token];
};

export const resolveShadow = (token?: ShadowToken): number => {
    if (!token) return 0;

    const shadowMap: Record<ShadowToken, number> = {
        none: 0,
        sm: 1,
        md: 2,
        lg: 4
    };

    return shadowMap[token];
};

export const convertToSx = (props: StyleProps): SxProps<Theme> => {
    const sx: SxProps<Theme> = {
        ...props,
        ...(props.m && { m: resolveSpacing(props.m) }),
        ...(props.mt && { mt: resolveSpacing(props.mt) }),
        ...(props.mr && { mr: resolveSpacing(props.mr) }),
        ...(props.mb && { mb: resolveSpacing(props.mb) }),
        ...(props.ml && { ml: resolveSpacing(props.ml) }),
        ...(props.mx && { mx: resolveSpacing(props.mx) }),
        ...(props.my && { my: resolveSpacing(props.my) }),
        ...(props.p && { p: resolveSpacing(props.p) }),
        ...(props.pt && { pt: resolveSpacing(props.pt) }),
        ...(props.pr && { pr: resolveSpacing(props.pr) }),
        ...(props.pb && { pb: resolveSpacing(props.pb) }),
        ...(props.pl && { pl: resolveSpacing(props.pl) }),
        ...(props.px && { px: resolveSpacing(props.px) }),
        ...(props.py && { py: resolveSpacing(props.py) }),
        ...(props.color && { color: resolveColor(props.color) }),
        ...(props.bgColor && { bgcolor: resolveColor(props.bgColor) }),
        ...(props.borderColor && { borderColor: resolveColor(props.borderColor) }),
        ...(props.fontSize && { fontSize: resolveFontSize(props.fontSize) }),
        ...(props.fontWeight && { fontWeight: resolveFontWeight(props.fontWeight) }),
        ...(props.borderRadius && { borderRadius: resolveBorderRadius(props.borderRadius) }),
        ...(props.shadow && { boxShadow: resolveShadow(props.shadow) }),
        ...(props.gap && { gap: resolveSpacing(props.gap) }),
        ...(props.gridGap && { gridGap: resolveSpacing(props.gridGap) }),
    };

    return sx;
};