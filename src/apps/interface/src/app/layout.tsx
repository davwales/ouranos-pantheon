import { Footer } from "@/app/components/footer";
import { NavBarActionsProvider } from "@/app/components/nav-bar-actions-context";
import { NavigationBarItem } from "@/app/components/responsive-navigation-bar";
import ResponsiveNavigationBar from "@/app/components/responsive-navigation-bar/responsive-navigation-bar";
import { ThemeProvider } from "@/app/components/theme-provider";
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
      {
        label: "Groups",
        description:
          "Organize symbols into custom groups like wishlists or categories to browse related items together.",
        href: "/hermes/groups",
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
