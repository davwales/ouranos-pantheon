// components/ResponsiveNavigationBar.tsx
"use client";

import { AppBar, Box, Toolbar, Typography, useMediaQuery, useTheme } from '@mui/material';
import DesktopNavigation from './desktop_navigation';
import MobileNavigation from './mobile_navigation';
import OuranosIcon from './ouranos_icon';

export default function ResponsiveNavigationBar() {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  return (
    <AppBar position="sticky">
      <Toolbar>
        <Box sx={{ flexGrow: 1 }}>
          {!isMobile ? <DesktopNavigation /> : <MobileNavigation />}
        </Box>
        <Box display="flex" alignItems="center">
          <Typography variant="h4" component="div">
            Ouranos
          </Typography>
          <OuranosIcon sx={{ ml: "1rem" }} />
        </Box>
      </Toolbar>
    </AppBar>
  );
};
