import { Box, CssBaseline } from '@mui/material';
import { AppRouterCacheProvider } from '@mui/material-nextjs/v14-appRouter';
import { ThemeProvider } from '@mui/material/styles';
import type { Metadata } from 'next';
import GlobalAlert from './components/Alerts/global_alert';
import ResponsiveNavigationBar from './components/responsive_navigation_bar';
import siteTheme from './site_theme';

export const metadata: Metadata = {
  title: 'Ouranos',
  description: 'UI to interact with Ouranos applications.'
}

export default function RootLayout({ children }: React.PropsWithChildren) {
  return (
    <html lang="en" style={{
      height: "100%",
      margin: 0,
      padding: 0
    }}>
      <body style={{
        height: "100%",
        margin: 0,
        padding: 0
      }}>
        <AppRouterCacheProvider>
          <ThemeProvider theme={siteTheme}>
            <CssBaseline />
            <GlobalAlert />
            <Box sx={{
              minHeight: '100vh',
              margin: 0,
              display: 'flex',
              flexDirection: 'column'
            }}>
              <ResponsiveNavigationBar />
              <Box sx={{ m: '1rem' }}>
                {children}
              </Box>
            </Box>
          </ThemeProvider>
        </AppRouterCacheProvider>
      </body>
    </html>
  )
}
