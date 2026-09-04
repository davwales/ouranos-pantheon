import { Footer } from "@/components/shared/footer";
import { NavBarActionsProvider } from "@/components/shared/nav-bar-actions-context";
import { NavigationBarItem } from "@/components/shared/responsive-navigation-bar";
import ResponsiveNavigationBar from "@/components/shared/responsive-navigation-bar/responsive-navigation-bar";
import { ThemeProvider } from "@/components/shared/theme-provider";
import { SidebarProvider } from "@/components/ui/sidebar";
import type { Metadata, Viewport } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Ouranos",
  description: "UI to interact with Ouranos applications.",
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  interactiveWidget: "resizes-content",
};

const navigationItems: NavigationBarItem[] = [
  {
    label: "Home",
    options: [
      {
        label: "",
        href: "/",
      },
    ],
  },
  {
    label: "Hermes",
    options: [
      {
        label: "Chat",
        description: "Have a conversation with a virtual assistant.",
        href: "/hermes/chat",
      },
      {
        label: "Saved Conversations",
        description: "View and resume your saved conversations.",
        href: "/hermes/conversations",
      },
      {
        label: "Manage Personas",
        description: "Configure personas you can interact with.",
        href: "/hermes/personas",
      },
      {
        label: "Manage Models",
        description: "Configure LLM models and their parameters.",
        href: "/hermes/models",
      },
      {
        label: "Manage Traits",
        description: "Create and manage conversation traits.",
        href: "/hermes/traits",
      },
    ],
  },
  {
    label: "Plutus",
    options: [
      {
        label: "Markets",
        description: "Select a market to explore and analyze.",
        href: "/plutus",
      },
    ],
  },
  {
    label: "Hestia",
    options: [
      {
        label: "Recipes",
        description: "Browse your saved recipes.",
        href: "/hestia/recipes",
      },
      {
        label: "Shopping List",
        description: "Build a grocery list from saved recipes.",
        href: "/hestia/shopping-list",
      },
    ],
  },
];

export default function RootLayout({ children }: React.PropsWithChildren) {
  return (
    // See https://github.com/pacocoursey/next-themes?tab=readme-ov-file#with-app for why we need to suppress hydration.
    <html lang="en" suppressHydrationWarning>
      <body>
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          <NavBarActionsProvider>
            <SidebarProvider>
              <div className="w-full flex flex-col min-h-dvh h-dvh">
                <ResponsiveNavigationBar items={navigationItems} />
                <main className="flex-auto overflow-auto">{children}</main>
                <Footer />
              </div>
            </SidebarProvider>
          </NavBarActionsProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
