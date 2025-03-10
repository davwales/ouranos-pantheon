"use client";

import { ThemeProvider as MuiThemeProvider } from "@mui/material";
import { createTheme, Theme } from "@mui/material/styles";

interface ThemeProviderProps {
    children: React.ReactNode;
    mode?: 'light' | 'dark';
}

export default function ThemeProvider(props: ThemeProviderProps) {
    const siteTheme: Theme = createTheme({
        palette: {
            mode: props.mode ?? 'dark'
        }
    });

    return (
        <MuiThemeProvider theme={siteTheme}>
            {props.children}
        </MuiThemeProvider>
    );
}
