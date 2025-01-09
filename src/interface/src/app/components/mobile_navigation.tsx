// components/MobileNavigation.tsx
"use client";

import { ExpandLess, ExpandMore } from '@mui/icons-material';
import MenuIcon from '@mui/icons-material/Menu';
import {
    Box,
    Collapse,
    Drawer,
    IconButton,
    List,
    ListItemButton,
    ListItemText,
} from '@mui/material';
import Link from 'next/link';
import React, { useState } from 'react';

export default function MobileNavigation() {
    const [drawerOpen, setDrawerOpen] = useState(false);
    const [hermesOpen, setHermesOpen] = useState(false);

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

    const handleHermesClick = () => {
        setHermesOpen(!hermesOpen);
    };

    return (
        <>
            <IconButton
                color="inherit"
                edge="start"
                onClick={toggleDrawer(true)}
                sx={{ mr: 2 }}
            >
                <MenuIcon />
            </IconButton>
            <Drawer anchor="left" open={drawerOpen} onClose={toggleDrawer(false)}>
                <Box
                    sx={{ width: 250 }}
                    role="presentation"
                    onKeyDown={toggleDrawer(false)}
                >
                    <List component="nav">
                        <Link href="/" passHref legacyBehavior>
                            <ListItemButton component="a" onClick={toggleDrawer(false)}>
                                <ListItemText primary="Home" />
                            </ListItemButton>
                        </Link>
                        <Link href="/plutus" passHref legacyBehavior>
                            <ListItemButton component="a" onClick={toggleDrawer(false)}>
                                <ListItemText primary="Plutus" />
                            </ListItemButton>
                        </Link>
                        <ListItemButton onClick={handleHermesClick}>
                            <ListItemText primary="Hermes" />
                            {hermesOpen ? <ExpandLess /> : <ExpandMore />}
                        </ListItemButton>
                        <Collapse in={hermesOpen} timeout="auto" unmountOnExit>
                            <List component="div" disablePadding>
                                <Link href="/hermes/conversation" passHref legacyBehavior>
                                    <ListItemButton
                                        component="a"
                                        onClick={toggleDrawer(false)}
                                        sx={{ pl: 4 }}
                                    >
                                        <ListItemText primary="Create Conversation" />
                                    </ListItemButton>
                                </Link>
                                <Link href="/hermes/characters" passHref legacyBehavior>
                                    <ListItemButton
                                        component="a"
                                        onClick={toggleDrawer(false)}
                                        sx={{ pl: 4 }}
                                    >
                                        <ListItemText primary="Manage Characters" />
                                    </ListItemButton>
                                </Link>
                            </List>
                        </Collapse>
                    </List>
                </Box>
            </Drawer>
        </>
    );
};
