import { Footer } from '@/app/components/footer';
import { NavigationBarItem } from '@/app/components/responsive-navigation-bar';
import ResponsiveNavigationBar from '@/app/components/responsive-navigation-bar/responsive-navigation-bar';
import { ThemeProvider } from '@/app/components/theme-provider';
import { SidebarProvider } from '@/components/ui/sidebar';
import type { Metadata } from 'next';
import "./globals.css";

export const metadata: Metadata = {
  title: 'Ouranos',
  description: 'UI to interact with Ouranos applications.'
}

const navigationItems: NavigationBarItem[] = [
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
        description: "Have a conversation with the characters you have created.",
        href: "/hermes/conversation"
      },
      {
        label: "Manage Characters",
        description: "Create, edit, and delete characters.",
        href: "/hermes/characters"
      }
    ]
  },
  {
    label: "Plutus",
    options: [
      {
        label: "Explorer",
        description: "Explore symbols and their relevant market data over time.",
        href: "/plutus/explorer"
      },
      {
        label: "Recipes",
        description: "Manage and view how symbols can be combined to create other symbols.",
        href: "/plutus/recipes"
      },
      {
        label: "Forecasts",
        description: "View the predicted symbol prices for the coming days.",
        href: "/plutus/forecasts"
      },
      {
        label: "Recent Trades",
        description: "View trades for symbols as they are processed.",
        href: "/plutus/recent"
      },
    ]
  }
];

export default function RootLayout({ children }: React.PropsWithChildren) {
  return (
    <html lang="en">
      <body>
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          <SidebarProvider>
            <div className='w-full flex flex-col h-screen'>
              <ResponsiveNavigationBar items={navigationItems} />
              <main className="flex-auto overflow-auto">
                {children}
              </main>
              <Footer />
            </div>
          </SidebarProvider>
        </ThemeProvider>
      </body>
    </html>
  )
}
