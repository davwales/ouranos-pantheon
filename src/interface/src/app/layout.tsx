import Box from '@/app/components/core/layout/box';
import AppRouterCacheProvider from '@/app/components/core/utils/app_router_cache_provider';
import CssBaseline from '@/app/components/core/utils/css_baseline';
import ThemeProvider from '@/app/components/core/utils/theme_provider';
import GlobalAlert from '@/app/components/feedback/global_alert';
import ResponsiveNavigationBar from '@/app/components/navigation/navigation_bars/responsive_navigation_bar';
import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Ouranos',
  description: 'UI to interact with Ouranos applications.'
}

export default function RootLayout({ children }: React.PropsWithChildren) {
  const navigationItems = [
    {
      label: "Home",
      options: [
        {
          label: "",
          href: "/"
        }
      ]
    },
    {
      label: "Hermes",
      options: [
        {
          label: "Create Conversation",
          href: "/hermes/conversation"
        },
        {
          label: "Manage Characters",
          href: "/hermes/characters"
        }
      ]
    },
    {
      label: "Plutus",
      options: [
        {
          label: "Explorer",
          href: "/plutus/explorer"
        },
        {
          label: "Recent Trades",
          href: "/plutus/recent"
        }
      ]
    }
  ];

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
          <ThemeProvider>
            <CssBaseline />
            <GlobalAlert />
            <Box styling={{
              minHeight: '100vh',
              m: 'none',
              display: 'flex',
              flexDirection: 'column'
            }}>
              <ResponsiveNavigationBar items={navigationItems} />
              <Box styling={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column'
              }}>
                {children}
              </Box>
            </Box>
          </ThemeProvider>
        </AppRouterCacheProvider>
      </body>
    </html>
  )
}
