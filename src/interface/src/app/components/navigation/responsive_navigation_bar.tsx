"use client";

import Typography from '@/app/components/core/data-display/typography';
import Box from '@/app/components/core/layout/box';
import AppBar from '@/app/components/core/surfaces/app_bar';
import { useMobile } from '@/app/components/core/utils/breakpoints';
import OuranosIcon from '@/app/components/icons/ouranos_icon';
import DesktopNavigation from '@/app/components/navigation/desktop_navigation';
import MobileNavigation from '@/app/components/navigation/mobile_navigation';

export default function ResponsiveNavigationBar() {
  const isMobile = useMobile();

  return (
    <AppBar>
      <Box styling={{ flexGrow: 1 }}>
        {!isMobile ? <DesktopNavigation /> : <MobileNavigation />}
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
