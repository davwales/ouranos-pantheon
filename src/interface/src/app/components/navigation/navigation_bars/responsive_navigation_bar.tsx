"use client";

import Typography from '@/app/components/core/data-display/typography';
import Box from '@/app/components/core/layout/box';
import AppBar from '@/app/components/core/surfaces/app_bar';
import { useMobile } from '@/app/components/core/utils/breakpoints';
import OuranosIcon from '@/app/components/icons/ouranos_icon';
import DesktopNavigationBar from '@/app/components/navigation/navigation_bars/desktop_navigation_bar';
import MobileNavigationBar from '@/app/components/navigation/navigation_bars/mobile_navigation_bar';
import { NavigationBarItem } from '@/app/components/navigation/navigation_bars/navigation_bar_items';

interface ResponsiveNavigationBarProps {
  items: NavigationBarItem[];
}

export default function ResponsiveNavigationBar(props: ResponsiveNavigationBarProps) {
  const isMobile = useMobile();

  return (
    <AppBar>
      <Box styling={{ flexGrow: 1 }}>
        {!isMobile ? <DesktopNavigationBar items={props.items} /> : <MobileNavigationBar items={props.items} />}
      </Box>

      <Box styling={{ display: "flex", alignItems: "center" }}>
        <Typography variant="h4">
          Ouranos
        </Typography>

        <OuranosIcon styling={{ ml: "small" }} />
      </Box>
    </AppBar>
  );
};
