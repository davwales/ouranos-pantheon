import type { Metadata } from 'next'
import siteTheme from './site_theme'
import GlobalAlert from './components/Alerts/global_alert'
import { AppRouterCacheProvider } from '@mui/material-nextjs/v14-appRouter';
import { ThemeProvider } from '@mui/material/styles';
import { Box, CssBaseline } from '@mui/material';
import ResponsiveNavigationBar from './components/responsive_navigation_bar';

export const metadata: Metadata = {
  title: 'Talos',
  description: 'UI to interact with Talos applications.'
}

export default function RootLayout({ children }: React.PropsWithChildren) {
  return (
    <html lang="en" style={{ height: "100%" }}>
      <body style={{ height: "100%" }} >
        <AppRouterCacheProvider>
          <ThemeProvider theme={siteTheme}>
            <CssBaseline />
            <GlobalAlert />
            <ResponsiveNavigationBar />
            <Box sx={{ m: "1rem" }}>
              {children}
            </Box>
          </ThemeProvider>
        </AppRouterCacheProvider>
      </body>
    </html>
  )
}
