"use client";

import List from '@/app/components/core/data-display/list';
import ListItemButton from '@/app/components/core/data-display/list_item_button';
import ListItemText from '@/app/components/core/data-display/list_item_text';
import Typography from '@/app/components/core/data-display/typography';
import ExpandLessIcon from '@/app/components/core/icons/expand_less_icon';
import ExpandMoreIcon from '@/app/components/core/icons/expand_more_icon';
import MenuIcon from '@/app/components/core/icons/menu_icon';
import Button from '@/app/components/core/inputs/button';
import IconButton from '@/app/components/core/inputs/icon_button';
import Box from '@/app/components/core/layout/box';
import Drawer from '@/app/components/core/navigation/drawer';
import Menu from '@/app/components/core/navigation/menu';
import MenuItem from '@/app/components/core/navigation/menu_item';
import AppBar from '@/app/components/core/surfaces/app_bar';
import { useMobile } from '@/app/components/core/utils/breakpoints';
import Collapse from '@/app/components/core/utils/collapse';
import OuranosIcon from '@/app/components/icons/ouranos_icon';
import Link from 'next/link';
import { Fragment, useState } from 'react';

interface NavigationBarItemOption {
  label: string;
  href: string;
}

interface NavigationBarItem {
  label: string;
  options: NavigationBarItemOption[];
}

interface NavigationBarProps {
  items: NavigationBarItem[];
}

function DesktopNavigationBar(props: NavigationBarProps) {
  const [anchorElements, setAnchorElements] = useState<Record<string, HTMLElement | undefined>>({});

  const handleMenuOpen = (label: string, element: HTMLElement) => {
    setAnchorElements(prev => ({
      ...prev,
      [label]: element
    }));
  };

  const handleMenuClose = (label: string) => {
    setAnchorElements(prev => ({
      ...prev,
      [label]: undefined
    }));
  };

  const singleItem = (item: NavigationBarItem, key: number) => (
    <Link key={key} href={item.options[0].href || "#"} passHref legacyBehavior>
      <Button color="inherit" component="a">
        {item.label}
      </Button>
    </Link>
  );

  const multiItem = (item: NavigationBarItem, key: number) => {
    const anchorEl = anchorElements[item.label];

    return (
      <Fragment key={key}>
        <Button
          color="inherit"
          onClick={(e) => handleMenuOpen(item.label, e.currentTarget)}
          endIcon={!anchorEl ? <ExpandMoreIcon /> : <ExpandLessIcon />}
        >
          {item.label}
        </Button>
        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={() => handleMenuClose(item.label)}
        >
          {item.options?.map((option, optionIndex) => (
            <Link
              key={optionIndex}
              href={option.href}
              passHref
              legacyBehavior
            >
              <MenuItem
                onClick={() => handleMenuClose(item.label)}
                component="a"
              >
                {option.label}
              </MenuItem>
            </Link>
          ))}
        </Menu>
      </Fragment>
    );
  };

  const renderNavigationItem = (item: NavigationBarItem, index: number) => {
    const isSingleItem = !item.options || item.options.length <= 1;
    return isSingleItem ? singleItem(item, index) : multiItem(item, index);
  };

  return (
    <Box styling={{ display: "flex", alignItems: "center" }}>
      {props.items.map((item, index) => renderNavigationItem(item, index))}
    </Box>
  );
}


function MobileNavigationBar(props: NavigationBarProps) {
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [openSections, setOpenSections] = useState<Record<string, boolean>>({});

  const toggleDrawer =
    (open: boolean) => (event: React.KeyboardEvent | React.MouseEvent) => {
      if (
        event.type === 'keydown' &&
        ((event as React.KeyboardEvent).key === 'Tab' ||
          (event as React.KeyboardEvent).key === 'Shift')
      ) {
        return;
      }
      setDrawerOpen(open);
    };

  const toggleSection = (label: string) => {
    setOpenSections(prev => ({
      ...prev,
      [label]: !prev[label]
    }));
  };

  const singleItem = (item: NavigationBarItem, key: number) => (
    <Link key={key} href={item.options[0].href || "#"} passHref legacyBehavior>
      <ListItemButton component="a" onClick={toggleDrawer(false)}>
        <ListItemText primary={item.label} />
      </ListItemButton>
    </Link>
  );

  const multiItem = (item: NavigationBarItem, key: number) => {
    const isOpen = !!openSections[item.label];

    return (
      <Fragment key={key}>
        <ListItemButton onClick={() => toggleSection(item.label)}>
          <ListItemText primary={item.label} />
          {isOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
        </ListItemButton>
        <Collapse in={isOpen}>
          <List disablePadding>
            {item.options?.map((option, optionIndex) => (
              <Link
                key={optionIndex}
                href={option.href}
                passHref
                legacyBehavior
              >
                <ListItemButton
                  component="a"
                  onClick={toggleDrawer(false)}
                  styling={{ pl: 'large' }}
                >
                  <ListItemText primary={option.label} />
                </ListItemButton>
              </Link>
            ))}
          </List>
        </Collapse>
      </Fragment>
    );
  };

  const renderNavigationItem = (item: NavigationBarItem, index: number) => {
    const isSingleItem = !item.options || item.options.length <= 1;
    return isSingleItem ? singleItem(item, index) : multiItem(item, index);
  };

  return (
    <>
      <IconButton
        color="inherit"
        edge="start"
        onClick={toggleDrawer(true)}
        styling={{ mr: 'large' }}
      >
        <MenuIcon />
      </IconButton>
      <Drawer anchor="left" open={drawerOpen} onClose={toggleDrawer(false)}>
        <Box
          styling={{ width: 250 }}
          role="presentation"
          onKeyDown={toggleDrawer(false)}
        >
          <List component="nav">
            {props.items.map((item, index) => renderNavigationItem(item, index))}
          </List>
        </Box>
      </Drawer>
    </>
  );
}

export default function NavigationBar(props: NavigationBarProps) {
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
