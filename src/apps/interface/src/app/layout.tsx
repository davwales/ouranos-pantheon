import { CommandPalette } from "@/app/_components/command-palette";
import { Footer } from "@/components/shared/footer";
import { Breadcrumbs } from "@/components/shared/breadcrumbs";
import { NavBarActionsProvider } from "@/components/shared/nav-bar-actions-context";
import { NAVIGATION_ITEMS } from "@/components/shared/responsive-navigation-bar";
import ResponsiveNavigationBar from "@/components/shared/responsive-navigation-bar/responsive-navigation-bar";
import { ThemeProvider } from "@/components/shared/theme-provider";
import { ThemeToggle } from "@/components/shared/theme-toggle";
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
                <ResponsiveNavigationBar
                  items={NAVIGATION_ITEMS}
                  utilities={
                    <>
                      <CommandPalette />
                      <ThemeToggle />
                    </>
                  }
                />
                <Breadcrumbs />
                <main className="relative flex-auto overflow-auto">{children}</main>
                <Footer />
              </div>
            </SidebarProvider>
          </NavBarActionsProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
