import { useMediaQuery, useTheme } from "@mui/material";

type Breakpoint = 'xs' | 'sm' | 'md' | 'lg' | 'xl';
type Direction = 'up' | 'down';

export function useBreakpointUp(size: Breakpoint) {
    const theme = useTheme();
    return useMediaQuery(theme.breakpoints.up(size));
}

export function useBreakpointDown(size: Breakpoint) {
    const theme = useTheme();
    return useMediaQuery(theme.breakpoints.down(size));
}

export function useMobile() {
    return useBreakpointDown('sm');
}
